using System.Collections.Generic;
using System.Collections.Specialized;
using System.Threading.Tasks;
using EmbedIO;
using EmbedIO.Routing;
using EmbedIO.WebApi;

namespace enigma
{
    namespace Http
    {
        public sealed class AdminController : WebApiController
        {
            /// <summary>
            /// 登陆管理员账号
            /// </summary>
            /// <param name="username">用户名</param>
            /// <param name="password">密码</param>
            /// <returns>是否成功</returns>
            [Route(HttpVerbs.Post, "/login")]
            public bool Login([FormField] string username, [FormField] string password)
            {
                if (username == HttpServer.Instance.Options.AdminUser &&
                    password == HttpServer.Instance.Options.AdminPassword)
                {
                    HttpContext.Session["Admin"] = true;
                    return true;
                }

                return false;
            }

            /// <summary>
            /// 获取时间标签列表
            /// </summary>
            /// <returns>时间标签列表</returns>
            [Route(HttpVerbs.Get, "/TimeMarks")]
            public IEnumerable<TimeMark> GetTimeMarks()
            {
                List<TimeMark> list = new List<TimeMark>();
                // TODO
                return list;
            }

            /// <summary>
            /// 检查是否登陆了
            /// </summary>
            private bool IsLogin(IHttpContext httpContext)
            {
                return httpContext.Session.TryGetValue("Admin", out var value)
                       && (bool)value;
            }
        }
    }
}