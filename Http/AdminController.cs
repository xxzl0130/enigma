using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Threading.Tasks;
using EmbedIO;
using EmbedIO.Routing;
using EmbedIO.WebApi;
using enigma.DataBase;
using Swan.Logging;

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
            public ApiResult Login([FormField] string username, [FormField] string password)
            {
                if (username == HttpServer.Instance.Options.AdminUser &&
                    password == HttpServer.Instance.Options.AdminPassword)
                {
                    HttpContext.Session["Admin"] = true;
                    return ApiResult.Success;
                }

                return ApiResult.Fail;
            }

            /// <summary>
            /// 添加或更新时间标签
            /// </summary>
            /// <returns>是否成功</returns>
            [Route(HttpVerbs.Post, "/TimeMark")]
            public async Task<ApiResult> AddTimeMark([JsonData] TimeMark timeMark)
            {
                if(!CheckLogin(HttpContext))
                    return ApiResult.Fail;
                try
                {
                    await DB.Instance.AddTimeMarkAsync(timeMark);
                }
                catch (Exception e)
                {
                    return new ApiResult(e);
                }

                return ApiResult.Success;
            }

            /// <summary>
            /// 删除时间标签
            /// </summary>
            /// <returns>是否成功</returns>
            [Route(HttpVerbs.Delete, "/TimeMark")]
            public async Task<ApiResult> DelTimeMark([FormField] int id)
            {
                if (!CheckLogin(HttpContext))
                    return ApiResult.Fail;
                try
                {
                    await DB.Instance.DelTimeMarkAsync(id);
                }
                catch (Exception e)
                {
                    return new ApiResult(e);
                }
                return ApiResult.Success;
            }

            /// <summary>
            /// 更新数据库统计表
            /// </summary>
            /// <returns>是否成功</returns>
            [Route(HttpVerbs.Put, "/total")]
            public async Task<ApiResult> UpdateTableTotal([JsonData] TimeMark timeMark)
            {
                if (!CheckLogin(HttpContext))
                    return ApiResult.Fail;
                try
                {
                    //await DB.Instance.update(timeMark);
                }
                catch (Exception e)
                {
                    return new ApiResult(e);
                }
                return ApiResult.Success;
            }

            /// <summary>
            /// 测试
            /// </summary>
            /// <returns>是否成功</returns>
            [Route(HttpVerbs.Get, "/test")]
            public ApiResult Test()
            {
                if (!CheckLogin(HttpContext))
                    return ApiResult.Fail;
                // TODO
                return ApiResult.Success;
            }

            /// <summary>
            /// 检查是否登陆了，未登录设置401无权限
            /// </summary>
            private bool CheckLogin(IHttpContext httpContext)
            {
                httpContext.Session.TryGetValue("Admin", out var value);
                if (value != null && (bool) value)
                {
                    return true;
                }
                else
                {
                    httpContext.Response.StatusCode = 401;
                    return false;
                }
            }
        }
    }
}