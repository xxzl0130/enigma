using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using System.Timers;
using System.Web;
using Titanium.Web.Proxy;
using Titanium.Web.Proxy.EventArguments;
using Titanium.Web.Proxy.Models;
using Newtonsoft.Json.Linq;
using GF_CipherSharp;
using Newtonsoft.Json;

namespace enigma
{
    namespace proxy
    {
        public class Proxy
        {
            /// <summary>
            /// 单例对象
            /// </summary>
            private static readonly Proxy _instance = new Proxy();

            /// <summary>
            /// 获取单例对象
            /// </summary>
            public static Proxy Instance => _instance;

            /// <summary>
            /// 代理服务器
            /// </summary>
            private ProxyServer _proxyServer;

            private ProxyEndPoint _endPoint;

            /// <summary>
            /// 妖精信息
            /// </summary>
            private class Fairy
            {
                /// <summary>
                /// 妖精的id
                /// </summary>
                public int fairy_id;

                /// <summary>
                /// 技能等级
                /// </summary>
                public int skill_lv;

                public Fairy(int id = 0, int lv = 0)
                {
                    fairy_id = id;
                    skill_lv = lv;
                }
            };

            private class TeamInfo
            {
                public Fairy fairy;

                /// <summary>
                /// 所在点位
                /// </summary>
                public int spot_id;

                public TeamInfo()
                {
                    spot_id = 0;
                }
            }

            private class UserInfo
            {
                public string Uid;
                public string Sign;
                public int timestamp;

                public TeamInfo[] TeamList;

                /// <summary>
                /// 妖精列表，Fairy
                /// </summary>
                public Dictionary<int, Fairy> FairyDict;

                public UserInfo(string uid = "", string sign = "", int time = 0)
                {
                    Uid = uid;
                    Sign = sign;
                    timestamp = time;
                    TeamList = new TeamInfo[21]; // 直接给20个梯队位置以防以后扩容，0保留
                    for (var i = 0; i < TeamList.Length; ++i)
                        TeamList[i] = new TeamInfo();
                    FairyDict = new Dictionary<int, Fairy>();
                }
            }

            /// <summary>
            /// 清理signDict用的定时器
            /// </summary>
            private Timer _signTimer;

            /// <summary>
            /// // 用户数据的dict，key为uid，value为sign,uid和时间戳，定时清理
            /// </summary>
            private ConcurrentDictionary<string, UserInfo> _userInfoDict = new ConcurrentDictionary<string, UserInfo>();

            /// <summary>
            /// 数据处理规则
            /// </summary>
            private JObject _ruleJObject;

            /// <summary>
            /// 要处理的url列表，从rule的json的key列表读取
            /// </summary>
            private List<string> _urlList;

            /// <summary>
            /// 处理数据的函数的代理定义
            /// </summary>
            /// <param name="request">请求字符串</param>
            /// <param name="response">回复字符串</param>
            private delegate void ProcessDelegate(string request, string response);

            /// <summary>
            /// 要特殊处理的url以及对应函数的字典
            /// </summary>
            private Dictionary<string, ProcessDelegate> _specialUrlDict;

            /// <summary>
            /// 解析到数据请求进一步处理的delegate
            /// </summary>
            /// <param name="jsonObject">json形式的数据</param>
            public delegate void DataHandler(JObject jsonObject);

            /// <summary>
            /// 解析到数据请求进一步处理的event
            /// </summary>
            public event DataHandler DataEvent;

            /// <summary>
            /// 本机IP地址
            /// </summary>
            public string LocalIPAddress;

            /// <summary>
            /// 游戏服务器host列表
            /// </summary>
            public static List<string> HostList = new List<string>
                {"sunborngame.com", "ppgame.com", "sn-game.txwy.tw", "girlfrontline.co.kr"};

            /// <summary>
            /// 登录获取uid的url列表
            /// </summary>
            public static List<string> UidList = new List<string>
                {"/Index/getDigitalSkyNbUid", "/Index/getUidTianxiaQueue", "/Index/getUidEnMicaQueue"};

            /// <summary>
            /// 日志接口，由外部提供实例
            /// </summary>
            public Serilog.ILogger Log = null;

            /// <summary>
            /// 构造函数
            /// </summary>
            private Proxy()
            {
                _proxyServer = new ProxyServer(false, false, false)
                {
                    TcpTimeWaitSeconds = 10,
                    ConnectionTimeOutSeconds = 15,
                    ReuseSocket = false,
                    EnableConnectionPool = false,
                    ForwardToUpstreamGateway = false
                };
                // 阻止生成pfx文件
                _proxyServer.CertificateManager.SaveFakeCertificates = false;
                _proxyServer.CertificateManager.RootCertificate = new X509Certificate2();
                _proxyServer.BeforeResponse += BeforeResponse;
                _proxyServer.BeforeRequest += BeforeRequest;
                _endPoint = new ExplicitProxyEndPoint(IPAddress.Any, 18888, false);
                _proxyServer.AddEndPoint(_endPoint);
                _signTimer = new Timer(Defines.SignExpireTime)
                    {AutoReset = true, Enabled = true, Interval = Defines.SignExpireTime};
                _signTimer.Elapsed += _signTimerElapsed;
                _ruleJObject =
                    (JObject) JsonConvert.DeserializeObject(System.Text.Encoding.UTF8.GetString(Resource.DataProcess));
                _urlList = new List<string>();
                if (_ruleJObject != null)
                {
                    foreach (var it in _ruleJObject)
                    {
                        _urlList.Add(it.Key);
                    }
                }

                _specialUrlDict = new Dictionary<string, ProcessDelegate>
                {
                    {"Index/index", GetIndex},
                    {"Fairy/teamFairy", TeamFairy},
                    {"Mission/teamMove", TeamMove},
                    {"Mission/startMission", StartMission}
                };

                using (var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0))
                {
                    socket.Connect(Defines.NetTestIP, 65530);
                    LocalIPAddress = socket.LocalEndPoint is IPEndPoint endPoint ? endPoint.Address.ToString() : "";
                }
            }

            /// <summary>
            /// 清理到期的sign
            /// </summary>
            private void _signTimerElapsed(object sender, ElapsedEventArgs e)
            {
                var time = Utils.GetUTC();
                UserInfo tmp;
                foreach (var it in _userInfoDict)
                {
                    if (time - it.Value.timestamp > Defines.SignExpireTime)
                    {
                        Log?.Debug("Remove user (uid={uid})", it.Key);
                        _userInfoDict.TryRemove(it.Key, out tmp);
                    }
                }
            }

            /// <summary>
            /// 析构函数
            /// </summary>
            ~Proxy()
            {
                Stop();
            }

            /// <summary>
            /// 监听端口
            /// </summary>
            public int Port
            {
                get => _endPoint.Port;
                set
                {
                    var running = _proxyServer.ProxyRunning;
                    if (running)
                        _proxyServer.Stop();
                    _proxyServer.RemoveEndPoint(_endPoint);
                    _endPoint = new ExplicitProxyEndPoint(IPAddress.Any, value, false);
                    _proxyServer.AddEndPoint(_endPoint);
                    if (running)
                        _proxyServer.Start();
                }
            }

            /// <summary>
            /// 是否启用对非游戏请求的禁用
            /// </summary>
            public bool EnableBlocking = false;

            /// <summary>
            /// 启动代理
            /// </summary>
            public void Start()
            {
                if (!_proxyServer.ProxyRunning)
                    _proxyServer.Start();
            }

            /// <summary>
            /// 停止代理
            /// </summary>
            public void Stop()
            {
                if (_proxyServer.ProxyRunning)
                    _proxyServer.Stop();
            }

            /// <summary>
            /// Request前的处理函数，主要用于保存request body
            /// </summary>
            private async Task BeforeRequest(object sender, SessionEventArgs e)
            {
                var host = e.HttpClient.Request.Host;
                var url = e.HttpClient.Request.Url;
                Log?.Verbose("Request {url}", url);
                if (host == null)
                    return;
                if (HostList.Any(it => host.EndsWith(it)) &&
                    (UidList.Any(it => url.EndsWith(it)) ||
                     _urlList.Any(it => url.EndsWith(it)) ||
                     _specialUrlDict.Any(it => url.EndsWith(it.Key))))
                {
                    // 要先在request里读取body才能保存下来
                    var body = await e.GetRequestBody();
                    if (body.Length == 0) return;
                    e.HttpClient.Request.KeepBody = true;
                    Log?.Debug("Save request body {url}, {length} bytes.", url, body.Length);

                    return;
                }

                // 心跳包
                if (url.EndsWith("/Index/heartBeat"))
                {
                    var body = await e.GetRequestBodyAsString();
                    if (body.Length == 0) return;
                    var uid = GetUidFromRequest(body);
                    if (_userInfoDict.ContainsKey(uid))
                    {
                        var tmp = _userInfoDict[uid];
                        tmp.timestamp = Utils.GetUTC();
                        _userInfoDict[uid] = tmp;
                    }

                    Log?.Debug("Heart beat packet, uid = {uid}.", uid);

                    return;
                }

                // 此时是没有匹配
                if (EnableBlocking)
                {
                    e.Ok("Blocked!");
                    Log?.Debug("Blocked {url}", url);
                }
            }

            /// <summary>
            /// Response处理函数
            /// </summary>
            private async Task BeforeResponse(object sender, SessionEventArgs e)
            {
                try
                {
                    var host = e.HttpClient.Request.Host;
                    var url = e.HttpClient.Request.Url;
                    Log?.Verbose("Response {url}", url);

                    if (host == null)
                        return;
                    if (HostList.All(it => !host.EndsWith(it)))
                        return; // 不在host列表里
                    if (!(UidList.Any(it => url.EndsWith(it)) ||
                          _urlList.Any(it => url.EndsWith(it)) ||
                          _specialUrlDict.Any(it => url.EndsWith(it.Key))))
                        return; // 不在要处理的列表里

                    Log?.Information("Process {url}", url);

                    var requestString = await e.GetRequestBodyAsString();
                    var responseString = await e.GetResponseBodyAsString();
                    Log?.Verbose("{url} : request = {request} , response = {response}", url, requestString,
                        responseString);

                    if (UidList.Any(it => url.EndsWith(it)))
                    {
                        GetUid(requestString, responseString);
                        return;
                    }

                    // 特殊处理
                    foreach (var it in _specialUrlDict.Where(it => url.EndsWith(it.Key)))
                    {
                        it.Value?.Invoke(requestString, responseString);
                        return;
                    }

                    // 按规则批量处理
                    foreach (var it in _urlList.Where(it => url.EndsWith(it)))
                    {
                        ProcessData(requestString, responseString, _ruleJObject[it], it);
                        return;
                    }
                }
                catch (Exception err)
                {
                    Log?.Warning(err.ToString());
                }
            }

            /// <summary>
            /// 解析sign信息并将其保存到dict中
            /// </summary>
            private void GetUid(string request, string response)
            {
                if (!response.StartsWith("#"))
                    return;
                var data = Cipher.DecodeDefault(response);
                if (data == "")
                    return;
                var obj = (JObject) JsonConvert.DeserializeObject(data);
                if (obj == null || !obj.ContainsKey(Defines.Sign) || !obj.ContainsKey(Defines.Uid))
                    return;
                _userInfoDict[obj[Defines.Uid].Value<string>()] = new UserInfo(obj[Defines.Uid].Value<string>(),
                    obj[Defines.Sign].Value<string>(), Utils.GetUTC());
                Log?.Debug("Get uid info: {uid}", _userInfoDict[obj[Defines.Uid].Value<string>()].Uid);
            }

            /// <summary>
            /// 从request中获取uid
            /// </summary>
            private string GetUidFromRequest(string request)
            {
                var parsedReq = HttpUtility.ParseQueryString(request);
                var uid = parsedReq[Defines.Uid];
                return uid;
            }

            /// <summary>
            /// 从request中获取uid并进一步获取UserInfo
            /// </summary>
            private UserInfo GetUserInfo(string request)
            {
                var uid = GetUidFromRequest(request);
                if (uid == null || !_userInfoDict.ContainsKey(uid))
                    return null;
                var user = _userInfoDict[uid];
                return user;
            }

            /// <summary>
            /// 更新用户信息的时间戳然后更新到dict中
            /// </summary>
            private void UpdateUserInfo(UserInfo user)
            {
                user.timestamp = Utils.GetUTC();
                _userInfoDict[user.Uid] = user;
            }

            /// <summary>
            /// 获取解析过的request数据
            /// </summary>
            private static NameValueCollection GetParsedRequest(string request)
            {
                var parsed = HttpUtility.ParseQueryString(request);
                return parsed;
            }

            /// <summary>
            /// 获取用户信息
            /// </summary>
            private UserInfo GetUserInfo(NameValueCollection parsed)
            {
                var uid = parsed?[Defines.Uid];
                if (uid == null || !_userInfoDict.ContainsKey(uid))
                    return null;
                var user = _userInfoDict[uid];
                return user;
            }

            /// <summary>
            /// 解析index游戏初始化数据
            /// </summary>
            private void GetIndex(string request, string response)
            {
                var user = GetUserInfo(request);
                if (user == null)
                    return;
                Log?.Information("Process Index with user {user}", user.Uid);
                var data = Cipher.Decode(response, user.Sign);
                if (data == "")
                    return;
                var index = (JObject) JsonConvert.DeserializeObject(data);
                if (index == null)
                    return;
                var fairy = index.Value<JObject>("fairy_with_user_info");
                user.FairyDict = new Dictionary<int, Fairy>();
                foreach (var it in fairy)
                {
                    var fairy_id = int.Parse(it.Value.Value<string>("fairy_id"));
                    var skill_lv = int.Parse(it.Value.Value<string>("skill_lv"));
                    var fairy_info = new Fairy(fairy_id, skill_lv);
                    user.FairyDict[int.Parse(it.Key)] = fairy_info;
                    var team_id = int.Parse(it.Value.Value<string>("team_id"));
                    if (team_id > 0 && team_id < user.TeamList.Length - 1)
                    {
                        user.TeamList[team_id].fairy = fairy_info;
                    }
                }

                UpdateUserInfo(user);
            }

            /// <summary>
            /// 更换妖精
            /// </summary>
            private void TeamFairy(string request, string response)
            {
                var parsed = GetParsedRequest(request);
                var user = GetUserInfo(parsed);
                if (user == null)
                    return;
                Log?.Information("Process TeamFairy with user {user}", user.Uid);
                var body = parsed[Defines.OutDataKey];
                var data = Cipher.Decode(body, user.Sign, false);
                if (data == "")
                    return;
                var obj = (JObject) JsonConvert.DeserializeObject(data);
                var team_id = obj.Value<int>("team_id");
                var fairy = obj.Value<int>("fairy_with_user_id");
                if (team_id > 0 && team_id < user.TeamList.Length - 1 && user.FairyDict.ContainsKey(fairy))
                {
                    user.TeamList[team_id].fairy = user.FairyDict[fairy];
                }

                UpdateUserInfo(user);
            }

            /// <summary>
            /// 梯队移动
            /// </summary>
            private void TeamMove(string request, string response)
            {
                var parsed = GetParsedRequest(request);
                var user = GetUserInfo(parsed);
                if (user == null)
                    return;
                Log?.Information("Process TeamMove with user {user}", user.Uid);
                var body = parsed[Defines.OutDataKey];
                var data = Cipher.Decode(body, user.Sign, false);
                if (data == "")
                    return;
                var obj = (JObject) JsonConvert.DeserializeObject(data);
                if (obj.Value<int>("person_type") != 1)
                    return;
                var team_id = obj.Value<int>("person_id");
                var spot = obj.Value<int>("to_spot_id");
                if (team_id > 0 && team_id < user.TeamList.Length - 1)
                {
                    user.TeamList[team_id].spot_id = spot;
                }

                UpdateUserInfo(user);
            }

            private void StartMission(string request, string response)
            {
                var parsed = GetParsedRequest(request);
                var user = GetUserInfo(parsed);
                if (user == null)
                    return;
                Log?.Information("Process StartMission with user {user}", user.Uid);
                var body = parsed[Defines.OutDataKey];
                var data = Cipher.Decode(body, user.Sign, false);
                if (data == "")
                    return;
                var obj = (JObject) JsonConvert.DeserializeObject(data);
                if (!obj.ContainsKey("spots"))
                    return;
                var spots = obj.Value<JArray>("spots");
                foreach (var it in spots)
                {
                    var spot_id = it.Value<int>("spot_id");
                    var team_id = it.Value<int>("team_id");
                    if (team_id > 0 && team_id < user.TeamList.Length - 1)
                    {
                        user.TeamList[team_id].spot_id = spot_id;
                    }
                }

                UpdateUserInfo(user);
            }

            /// <summary>
            /// 根据rule提取单个数据
            /// </summary>
            /// <param name="rule">规则</param>
            /// <param name="src">原始数据</param>
            /// <returns></returns>
            private JToken ExtractObject(JToken rule, JToken src)
            {
                JToken obj = src.DeepClone();
                // 循环递归查找
                foreach (var layer in rule)
                {
                    var key = layer.Value<string>();
                    if (key == "*")
                        break;
                    obj = obj[key];
                    if (obj == null)
                        break;
                }

                return obj;
            }

            /// <summary>
            /// 根据rule处理数据
            /// </summary>
            /// <param name="request">请求数据</param>
            /// <param name="response">响应数据</param>
            /// <param name="rule">规则</param>
            /// <param name="type">规则类型，也是url后缀</param>
            /// <returns></returns>
            private void ProcessData(string request, string response, JToken rule, string type)
            {
                if (rule == null)
                    return;
                var parsedReq = GetParsedRequest(request);
                var user = GetUserInfo(parsedReq);
                if (user == null)
                    return;

                Log?.Information("Process {rule} with user {user}", type, user.Uid);

                var dataJObject = new JObject();
                JToken reqObj = null, respObj = null;
                while (true)
                {
                    var outCode = parsedReq[Defines.OutDataKey];
                    if (outCode == null)
                        break;
                    var data = Cipher.Decode(outCode, user.Sign, false);
                    if (data == "")
                        break;
                    reqObj = (JToken) JsonConvert.DeserializeObject(data);
                    var reqRule = rule.Value<JObject>("request");
                    foreach (var it in reqRule)
                    {
                        var obj = ExtractObject(it.Value, reqObj);

                        if (obj != null)
                            dataJObject[it.Key] = obj;
                    }

                    break;
                }

                while (true)
                {
                    var respBody = Cipher.Decode(response, user.Sign);
                    if (respBody == "")
                        break;
                    respObj = (JToken) JsonConvert.DeserializeObject(respBody);
                    var respRule = rule.Value<JObject>("response");
                    foreach (var it in respRule)
                    {
                        var obj = ExtractObject(it.Value, respObj);
                        if (obj != null)
                            dataJObject[it.Key] = obj;
                    }

                    break;
                }

                dataJObject["uid"] = user.Uid;
                dataJObject["type"] = type;
                // 更新时间戳
                dataJObject["timestamp"] = Utils.GetUTC();

                UpdateUserInfo(user);

                #region 数据特殊处理

                switch (type)
                {
                    case "Mission/battleFinish":
                    {
                        // 敌人没死的战斗就没有意义了
                        if (reqObj == null || reqObj.Value<bool>("if_enemy_die") == false)
                            return;
                        // 处理妖精信息
                        if (dataJObject.Value<bool>("use_fairy_skill"))
                        {
                            var spot_id = dataJObject.Value<int>("spot_id");
                            // 记录使用的妖精的信息
                            foreach (var team in user.TeamList)
                            {
                                if (team.spot_id != spot_id) continue;
                                dataJObject[Defines.FairySkillLvKey] = team.fairy.skill_lv;
                                dataJObject[Defines.UseFairyIdKey] = team.fairy.fairy_id;
                                break;
                            }
                        }

                        // 拿到本轮死的enemy编号
                        var enemy = dataJObject.Value<JArray>("enemy");
                        dataJObject["enemy"] = enemy[enemy.Count - 1];

                        if (dataJObject.ContainsKey("battle_get_gun"))
                        {
                            // 仅保留gun_id
                            var battle_get_gun = dataJObject.Value<JArray>("battle_get_gun");
                            var guns = new JArray();
                            foreach (var gun in battle_get_gun)
                            {
                                guns.Add(gun["gun_id"]);
                            }

                            dataJObject["battle_get_gun"] = guns;
                        }

                        if (dataJObject.ContainsKey("battle_get_equip"))
                        {
                            // 仅保留equip_id
                            var battle_get_equip = dataJObject.Value<JArray>("battle_get_equip");
                            var equips = new JArray();
                            foreach (var equip in battle_get_equip)
                            {
                                equips.Add(equip["equip_id"]);
                            }

                            dataJObject["battle_get_equip"] = equips;
                        }

                        // 可能会在战斗中结束战役，调用战役结束处理。
                        if (respObj?["mission_win_result"] != null)
                        {
                            ProcessData(request, response, _ruleJObject["Mission/endTurn"], "Mission/endTurn");
                        }

                        break;
                    }
                    case "Mission/endTurn":
                    {
                        if (!dataJObject.ContainsKey("reward_gun"))
                            break;
                        if (!dataJObject.ContainsKey("mission_rank")) //TODO Check here
                            return; // 中间过场的数据是不需要的，只要整场战役结束的数据
                        if (respObj?["spot_act_info"]?[0] == null)
                            return;
                        // 记录一个spot_id用于查询战役
                        dataJObject["spot_id"] = respObj["spot_act_info"][0]["spot_id"];

                        if (dataJObject["reward_gun"] != null)
                        {
                            // 仅保留equip_id
                            var reward_gun = dataJObject.Value<JArray>("reward_gun");
                            var guns = new JArray();
                            foreach (var gun in reward_gun)
                            {
                                guns.Add(gun["gun_id"]);
                            }

                            dataJObject["reward_gun"] = guns;
                        }

                        if (dataJObject["reward_equip"] != null)
                        {
                            // 仅保留gun_id
                            var reward_equip = dataJObject.Value<JArray>("reward_equip");
                            var equips = new JArray();
                            foreach (var equip in reward_equip)
                            {
                                equips.Add(equip["equip_id"]);
                            }

                            dataJObject["reward_equip"] = equips;
                        }

                        break;
                    }
                }

                #endregion

                Log?.Debug("Processed {rule} with user {user}:\n{data}", type, user.Uid, dataJObject);
                // 调用数据后处理
                DataEvent?.Invoke(dataJObject);
            }
        }
    }
}
