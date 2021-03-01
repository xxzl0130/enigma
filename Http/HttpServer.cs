using System;
using EmbedIO;

namespace enigma
{
    namespace Http
    {
        public class HttpServer
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
                /// 端口
                /// </summary>
                public int Port = 9988;
                /// <summary>
                /// SSL证书(pfx格式)文件
                /// </summary>
                public string PfxFile = string.Empty;
                /// <summary>
                /// SSL证书密码
                /// </summary>
                public string PfxPassword = string.Empty;
            }

            /// <summary>
            /// 启动Http服务
            /// </summary>
            /// <param name="options">参数</param>
            public void Start(HttpOptions options)
            {
                // TODO
            }

            public void Stop()
            {
                if (_server == null) 
                    return;
                _server.Listener.Stop();
                _server.Dispose();
                _server = null;
            }
        }
    }
}