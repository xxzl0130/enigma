using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Timers;
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
            // 单例对象
            private static readonly Proxy _instance = new Proxy();

            /// <summary>
            /// 获取单例对象
            /// </summary>
            public static Proxy Instance => _instance;

            // 代理服务器
            private ProxyServer _proxyServer;
            private ProxyEndPoint _endPoint;

            // 清理signDict用的定时器
            private Timer _signTimer;
            // sign过期时间，10分钟
            private static int _signExpireTime = 30 * 60 * 1000;
            // sign数据的dict，key为IP，value为sign,uid和时间戳，定时清理
            private ConcurrentDictionary<string, Tuple<string, string, int>> _signDict = new ConcurrentDictionary<string, Tuple<string, string, int>>();

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
                _signTimer = new Timer(_signExpireTime) {AutoReset = true, Enabled = true};
                _signTimer.Elapsed += _signTimerElapsed;
            }

            // 清理到期的sign
            private void _signTimerElapsed(object sender, ElapsedEventArgs e)
            {
                var time = Utils.GetUTC();
                Tuple<string, string, int> tmp;
                foreach (var it in _signDict)
                {
                    if (time - it.Value.Item3 > _signExpireTime)
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
                if(host == null)
                    return;
                if (HostList.Any(it => host.EndsWith(it)))
                {
                    var body = await e.GetRequestBody();
                    if (body.Length == 0) return;
                    var req = e.HttpClient.Request;
                    req.KeepBody = true;
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
                var reqBody = await e.GetRequestBodyAsString();
                var respBody = await e.GetResponseBodyAsString();
                // TODO 更多处理
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
                var ip = e.ClientRemoteEndPoint.Address.ToString();
                _signDict[ip] = new Tuple<string, string, int>(
                    obj["sign"].Value<string>(), obj["uid"].Value<string>(), Utils.GetUTC());
            }
        }
    }
}
