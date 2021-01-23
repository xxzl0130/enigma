using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
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
            // sign过期时间，30分钟
            private static int _signExpireTime = 30 * 60 * 1000;
            // 单例对象
            private static readonly Proxy _instance = new Proxy();

            /// <summary>
            /// 获取单例对象
            /// </summary>
            public static Proxy Instance => _instance;

            // 代理服务器
            private ProxyServer _proxyServer;
            private ProxyEndPoint _endPoint;

            private struct UserInfo
            {
                public string Uid;
                public string Sign;
                public int timestamp;

                public UserInfo(string uid = "", string sign = "", int time = 0)
                {
                    Uid = uid;
                    Sign = sign;
                    timestamp = time;
                }
            }

            // 清理signDict用的定时器
            private Timer _signTimer;
            // sign数据的dict，key为uid，value为sign,uid和时间戳，定时清理
            private ConcurrentDictionary<string, UserInfo> _signDict = new ConcurrentDictionary<string, UserInfo>();
            // 数据处理规则
            private JObject _ruleJObject;
            // 要处理的url列表，从rule的json的key列表读取
            private List<string> _urlList;

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
                _proxyServer.CertificateManager.SaveFakeCertificates = false;
                _proxyServer.BeforeResponse += BeforeResponse;
                _proxyServer.BeforeRequest += BeforeRequest;
                _endPoint = new ExplicitProxyEndPoint(IPAddress.Any, 18888, false);
                _proxyServer.AddEndPoint(_endPoint);
                _signTimer = new Timer(_signExpireTime) {AutoReset = true, Enabled = true, Interval = _signExpireTime};
                _signTimer.Elapsed += _signTimerElapsed;
                _ruleJObject = (JObject) JsonConvert.DeserializeObject(System.Text.Encoding.UTF8.GetString(Resource.DataProcess));
                _urlList = new List<string>();
                if (_ruleJObject != null)
                {
                    foreach (var it in _ruleJObject)
                    {
                        _urlList.Add(it.Key);
                    }
                }

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
                foreach (var it in _signDict)
                {
                    if (time - it.Value.timestamp > _signExpireTime)
                    {
                        _signDict.TryRemove(it.Key,out tmp);
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
                if(!_proxyServer.ProxyRunning)
                    _proxyServer.Start();
            }

            /// <summary>
            /// 停止代理
            /// </summary>
            public void Stop()
            {
                if(_proxyServer.ProxyRunning)
                    _proxyServer.Stop();
            }

            /// <summary>
            /// Request前的处理函数，主要用于保存request body
            /// </summary>
            private async Task BeforeRequest(object sender, SessionEventArgs e)
            {
                var host = e.HttpClient.Request.Host;
                var url = e.HttpClient.Request.Url;
                if(host == null)
                    return;
                if (HostList.Any(it => host.EndsWith(it)) &&
                    (_urlList.Any(it => url.EndsWith(it)) ||
                     url.EndsWith("/Index/index"))) 
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
                    if (_signDict.ContainsKey(uid))
                    {
                        var tmp = _signDict[uid];
                        tmp.timestamp = Utils.GetUTC();
                        _signDict[uid] = tmp;
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
                var host = e.HttpClient.Request.Host;
                var url = e.HttpClient.Request.Url;

                if(host == null)
                    return;
                if (HostList.All(it => !host.EndsWith(it)))
                    return; // 不在host列表里

                if (UidList.Any(it => url.EndsWith(it)))
                {
                    await GetUid(e);
                    return;
                }

                foreach (var it in _urlList.Where(it => url.EndsWith(it)))
                {
                    await ProcessData(e, _ruleJObject[it], it);
                    return;
                }
            }

            /// <summary>
            /// 解析sign信息并将其保存到dict中
            /// </summary>
            private async Task GetUid(SessionEventArgs e)
            {
                var respBody = await e.GetResponseBodyAsString();
                var data = Cipher.DecodeDefault(respBody);
                if (data == "")
                    return;
                var obj = (JObject) JsonConvert.DeserializeObject(data);
                if (obj == null || !obj.ContainsKey("sign") || !obj.ContainsKey("uid"))
                    return;
                _signDict[obj["uid"].Value<string>()] = new UserInfo(obj["uid"].Value<string>(), obj["sign"].Value<string>(), Utils.GetUTC());
            }

            /// <summary>
            /// 根据rule处理数据
            /// </summary>
            /// <param name="e">代理数据</param>
            /// <param name="rule">规则</param>
            /// <param name="type">规则类型，也是url后缀</param>
            /// <returns></returns>
            private async Task ProcessData(SessionEventArgs e, JToken rule, string type)
            {
                if(rule == null)
                    return;
                var reqBody = await e.GetRequestBodyAsString();
                var parsedReq = HttpUtility.ParseQueryString(reqBody);
                var uid = parsedReq["uid"];
                if(uid == null || !_signDict.ContainsKey(uid))
                    return;

                var user = _signDict[uid];

                var dataJObject = new JObject();

                try
                {
                    while (true)
                    {
                        var outCode = parsedReq["outdatacode"];
                        if (outCode == null)
                            break;
                        var data = Cipher.Decode(outCode, user.Sign, false);
                        if (data == "")
                            break;
                        var reqObj = (JObject) JsonConvert.DeserializeObject(data);
                        var reqRule = rule.Value<JObject>("request");
                        foreach (var it in reqRule)
                        {
                            var obj = reqObj.DeepClone();
                            var token = it.Value;
                            // 循环递归查找
                            foreach (var layer in token)
                            {
                                var key = layer.Value<string>();
                                obj = obj[key];
                                if (obj == null)
                                    break;
                            }

                            if(obj != null)
                                dataJObject[it.Key] = obj;
                        }

                        break;
                    }

                    while (true)
                    {
                        var respBody = Cipher.Decode(await e.GetResponseBodyAsString(), user.Sign);
                        if (respBody == "")
                            break;
                        var respObj = (JObject) JsonConvert.DeserializeObject(respBody);
                        var respRule = rule.Value<JObject>("response");
                        foreach (var it in respRule)
                        {
                            var token = it.Value;
                            var obj = respObj.DeepClone();
                            // 循环递归查找
                            foreach (var layer in token)
                            {
                                var key = layer.Value<string>();
                                obj = obj[key];
                                if(obj == null)
                                    break;
                            }

                            if (obj != null)
                                dataJObject[it.Key] = obj;
                        }

                        break;
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

                dataJObject["uid"] = uid;
                dataJObject["type"] = type;
                // 更新时间戳
                dataJObject["timestamp"] = user.timestamp = Utils.GetUTC();
                _signDict[uid] = user;

                // 调用数据后处理
                DataEvent?.Invoke(dataJObject);
            }
        }
    }
}
