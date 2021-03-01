using System;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using EmbedIO;
using EmbedIO.Files;
using EmbedIO.Security;
using EmbedIO.WebApi;
using Swan.Logging;

namespace enigma
{
    namespace Http
    {
        public sealed class HttpServer
        {
            /// <summary>
            /// 单例对象
            /// </summary>
            private static readonly HttpServer _instance = new HttpServer();

            /// <summary>
            /// 获取单例对象
            /// </summary>
            public static HttpServer Instance => _instance;

            /// <summary>
            /// EmbedIO服务器
            /// </summary>
            private WebServer _server = null;

            /// <summary>
            /// 需要从外部设置的Http参数
            /// </summary>
            public class HttpOptions
            {
                /// <summary>
                /// 监听端口
                /// </summary>
                public int Port = 9988;
                /// <summary>
                /// SSL证书(pfx格式)文件，为空时为Http
                /// </summary>
                public string PfxFile = string.Empty;
                /// <summary>
                /// SSL证书密码
                /// </summary>
                public string PfxPassword = string.Empty;
                /// <summary>
                /// 管理员用户名，默认Admin
                /// </summary>
                public string AdminUser = "Admin";
                /// <summary>
                /// 管理员密码，默认Admin
                /// </summary>
                public string AdminPassword = "Admin";
                /// <summary>
                /// 静态文件路由
                /// </summary>
                public string StaticFolderRoute = "/";
                /// <summary>
                /// 静态文件的本地路径，为空时不提供文件访问
                /// </summary>
                public string StaticFolderPath = string.Empty;
                /// <summary>
                /// 是否启用Http自带的log
                /// </summary>
                public bool HttpLog = false;
            }

            /// <summary>
            /// 从外部设置的Http参数
            /// </summary>
            public HttpOptions Options = new HttpOptions();
            /// <summary>
            /// HttpServer的取消接口
            /// </summary>
            private CancellationTokenSource _httpCts;

            /// <summary>
            /// 启动Http服务
            /// </summary>
            /// <param name="options">参数</param>
            public void Start(HttpOptions options = null)
            {
                if (options != null)
                    Options = options;
                Stop();
                _httpCts = new CancellationTokenSource();
                if (!Options.HttpLog)
                {
                    Logger.UnregisterLogger<ConsoleLogger>();
                }
                else
                {
                    Logger.RegisterLogger<ConsoleLogger>();
                }
                _server = new WebServer(cfg =>
                    {
                        cfg.WithMode(HttpListenerMode.EmbedIO);
                        if (!string.IsNullOrEmpty(Options.PfxFile))
                        {
                            cfg.WithUrlPrefix($"https://*:{Options.Port}");
                            cfg.WithCertificate(new X509Certificate2(Options.PfxFile, Options.PfxPassword));
                        }
                        else
                        {
                            cfg.WithUrlPrefix($"http://*:{Options.Port}");
                        }
                    })
                    .WithIPBanning(o => o
                        .WithMaxRequestsPerSecond(5)
                        .WithRegexRules("HTTP exception 404"))
                    .WithLocalSessionManager()
                    .WithWebApi("/api", m => m.WithController<HttpController>());
                if (!string.IsNullOrEmpty(Options.StaticFolderPath))
                {
                    _server.WithStaticFolder(Options.StaticFolderRoute, Options.StaticFolderPath,
                        false, m => m.WithContentCaching(true));
                }

                _server.RunAsync(_httpCts.Token);
            }

            /// <summary>
            /// 停止Http服务
            /// </summary>
            public void Stop()
            {
                if (_server == null) 
                    return;
                _httpCts.Cancel();
                _server.Dispose();
                _server = null;
            }
        }
    }
}