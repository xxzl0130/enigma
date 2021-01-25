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
            /// sign过期时间，30分钟
            /// </summary>
            private static int _signExpireTime = 30 * 60 * 1000;
            
            /// <summary>
            /// Request中的数据key
            /// </summary>
            private static string _OutDataKey = "outdatacode";

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

            private class TeamInfo
            {
                /// <summary>
                /// 妖精类型
                /// </summary>
                public int fairy_id;

                /// <summary>
                /// 所在点位
                /// </summary>
                public int spot_id;

                public TeamInfo(int fairy = 0, int spot = 0)
                {
                    fairy_id = fairy;
                    spot_id = spot;
                }
            }

            private class UserInfo
            {
                public string Uid;
                public string Sign;
                public int timestamp;

                public TeamInfo[] TeamList;

                /// <summary>
                /// 妖精列表，为id到fairy_id的对应
                /// </summary>
                public Dictionary<int, int> FairyDict;

                public UserInfo(string uid = "", string sign = "", int time = 0)
                {
                    Uid = uid;
                    Sign = sign;
                    timestamp = time;
                    TeamList = new TeamInfo[21]; // 直接给20个梯队位置以防以后扩容，0保留
                    FairyDict = new Dictionary<int, int>();
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
                _signTimer = new Timer(_signExpireTime) {AutoReset = true, Enabled = true, Interval = _signExpireTime};
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
                    {"/Index/index", GetIndex},
                    {"Fairy/teamFairy", TeamFairy},
                    {"Mission/teamMove", TeamMove},
                    {"Mission/startMission", StartMission}
                };

                using (var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0))
                {
                    socket.Connect("114.114.114.114", 65530);
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
                    if (time - it.Value.timestamp > _signExpireTime)
                    {
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
                if (host == null)
                    return;
                if (HostList.Any(it => host.EndsWith(it)) &&
                    (_urlList.Any(it => url.EndsWith(it)) ||
                     _specialUrlDict.Any(it=>url.EndsWith(it.Key))))
                {
                    // 要先在request里读取body才能保存下来
                    var body = await e.GetRequestBody();
                    if (body.Length == 0) return;
                    e.HttpClient.Request.KeepBody = true;

                    return;
                }

                // 心跳包
                if (url.EndsWith("/Index/heartBeat"))
                {
                    var body = await e.GetRequestBodyAsString();
                    if (body.Length == 0) return;
                    var parsed = HttpUtility.ParseQueryString(body);
                    var uid = parsed["uid"];
                    if (_userInfoDict.ContainsKey(uid))
                    {
                        var tmp = _userInfoDict[uid];
                        tmp.timestamp = Utils.GetUTC();
                        _userInfoDict[uid] = tmp;
                    }

                    return;
                }

                // 此时是没有匹配
                if (EnableBlocking)
                {
                    e.Ok("Blocked!");
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

                    if (host == null)
                        return;
                    if (HostList.All(it => !host.EndsWith(it)))
                        return; // 不在host列表里
                    if (_urlList.All(it => !url.EndsWith(it)) &&
                        _specialUrlDict.All(it => url.EndsWith(it.Key)))
                        return; // 不在要处理的列表里

                    var requestString = await e.GetRequestBodyAsString();
                    var responseString = await e.GetResponseBodyAsString();

                    if (UidList.Any(it => url.EndsWith(it)))
                    {
                        GetUid(requestString,responseString);
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
#if DEBUG
                catch (Exception err)
                {
                    Console.WriteLine(err.ToString());
                }
#else
                catch (Exception)
                {
                    // ignored
                }
#endif
            }

            /// <summary>
            /// 解析sign信息并将其保存到dict中
            /// </summary>
            private void GetUid(string request,string response)
            {
                var data = Cipher.DecodeDefault(request);
                if (data == "")
                    return;
                var obj = (JObject) JsonConvert.DeserializeObject(data);
                if (obj == null || !obj.ContainsKey("sign") || !obj.ContainsKey("uid"))
                    return;
                _userInfoDict[obj["uid"].Value<string>()] = new UserInfo(obj["uid"].Value<string>(),
                    obj["sign"].Value<string>(), Utils.GetUTC());
            }

            /// <summary>
            /// 从request中获取uid
            /// </summary>
            private string GetUidFromRequest(string request)
            {
                var parsedReq = HttpUtility.ParseQueryString(request);
                var uid = parsedReq["uid"];
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
            private NameValueCollection GetParsedRequest(string request)
            {
                var parsed = HttpUtility.ParseQueryString(request);
                return parsed;
            }

            /// <summary>
            /// 获取用户信息
            /// </summary>
            private UserInfo GetUserInfo(NameValueCollection parsed)
            {
                var uid = parsed?["uid"];
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
                var data = Cipher.Decode(request, user.Sign);
                if (data == "")
                    return;
                var index = (JObject) JsonConvert.DeserializeObject(data);
                if (index == null)
                    return;
                var fairy = index.Value<JObject>("fairy_with_user_info");
                user.FairyDict = new Dictionary<int, int>();
                foreach (var it in fairy)
                {
                    var fairy_id = int.Parse(it.Value.Value<string>("fairy_id"));
                    user.FairyDict[int.Parse(it.Key)] = fairy_id;
                    var team_id = int.Parse(it.Value.Value<string>("team_id"));
                    if (team_id > 0 && team_id < user.TeamList.Length - 1)
                    {
                        user.TeamList[team_id].fairy_id = fairy_id;
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
                var body = parsed[_OutDataKey];
                var data = Cipher.Decode(body, user.Sign);
                if (data == "")
                    return;
                var obj = (JObject)JsonConvert.DeserializeObject(data);
                var team_id = obj.Value<int>("team_id");
                var fairy = obj.Value<int>("fairy_with_user_id");
                if (team_id > 0 && team_id < user.TeamList.Length - 1 && user.FairyDict.ContainsKey(fairy))
                {
                    user.TeamList[team_id].fairy_id = user.FairyDict[fairy];
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
                var body = parsed[_OutDataKey];
                var data = Cipher.Decode(body, user.Sign);
                if (data == "")
                    return;
                var obj = (JObject)JsonConvert.DeserializeObject(data);
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
                var body = parsed[_OutDataKey];
                var data = Cipher.Decode(body, user.Sign);
                if (data == "")
                    return;
                var obj = (JObject)JsonConvert.DeserializeObject(data);
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
            /// <param name="e">代理数据</param>
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

                var dataJObject = new JObject();

                while (true)
                {
                    var outCode = parsedReq[_OutDataKey];
                    if (outCode == null)
                        break;
                    var data = Cipher.Decode(outCode, user.Sign, false);
                    if (data == "")
                        break;
                    var reqObj = (JToken) JsonConvert.DeserializeObject(data);
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
                    var respObj = (JToken) JsonConvert.DeserializeObject(respBody);
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

                // 调用数据后处理
                DataEvent?.Invoke(dataJObject);
            }
        }
    }
}
