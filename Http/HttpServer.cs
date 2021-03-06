﻿using System;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using EmbedIO;
using EmbedIO.Files;
using EmbedIO.Security;
using EmbedIO.WebApi;
using Swan.Logging;

namespace enigma.Http
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

#if DEBUG
        public const string PathPrefix = "../../../../HTML/";

        /// <summary>
        /// 数据文件存储路径
        /// </summary>
        public const string DataPath = PathPrefix + "Data";

        /// <summary>
        /// HTML文件存储路径
        /// </summary>
        public const string HtmlPath = PathPrefix + "HTML";

        /// <summary>
        /// 静态文件存储路径
        /// </summary>
        public const string StaticPath = PathPrefix + "Static";
#else
            /// <summary>
            /// 数据文件存储路径
            /// </summary>
            public const string DataPath = "./Data";
            /// <summary>
            /// HTML文件存储路径
            /// </summary>
            public const string HtmlPath = "./HTML";
            /// <summary>
            /// 静态文件存储路径
            /// </summary>
            public const string StaticPath = "./Static";
#endif

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
                    .WithMaxRequestsPerSecond(20)
                    .WithRegexRules("HTTP exception 404"))
                .WithLocalSessionManager(m => { m.SessionDuration = TimeSpan.FromDays(1); })
                //.WithCors()
#if DEBUG
                .WithWebApi("/api/admin", m => m.WithController<AdminController>())
                .WithStaticFolder("/static", StaticPath, false, m => m.WithContentCaching(false))
                .WithStaticFolder("/data", DataPath, false, m => m.WithContentCaching(false))
                .WithStaticFolder("/js", HtmlPath + "/js", false, m => m.WithContentCaching(false))
                .WithStaticFolder("/css", HtmlPath + "/css", false, m => m.WithContentCaching(false))
                .WithStaticFolder("/", HtmlPath, false, m => m.WithContentCaching(false));
#else
                    .WithWebApi("/api/admin", m => m.WithController<AdminController>())
                    .WithStaticFolder("/static", StaticPath, true, m => m.WithContentCaching(true))
                    .WithStaticFolder("/data", DataPath, false, m => m.WithContentCaching(true))
                    .WithStaticFolder("/js", HtmlPath + "/js", true, m => m.WithContentCaching(false))
                    .WithStaticFolder("/css", HtmlPath + "/css", true, m => m.WithContentCaching(true))
                    .WithStaticFolder("/", HtmlPath, true, m => m.WithContentCaching(true));
#endif

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