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
        internal sealed class HttpController : WebApiController
        {
            /// <summary>
            /// 获取时间标签列表
            /// </summary>
            /// <returns>时间标签列表</returns>
            [Route(HttpVerbs.Get, "/TimeMarks")]
            public ApiResult GetTimeMarks()
            {
                List<TimeMark> list = new List<TimeMark>(){new TimeMark(){ID = 233}};
                // TODO
                return new ApiResult(){Ok = true,Data = list};
            }
            /// <summary>
            /// test
            /// </summary>
            /// <returns>test</returns>
            [Route(HttpVerbs.Get, "/test")]
            public string test()
            {
                List<TimeMark> list = new List<TimeMark>();
                // TODO
                return "hello world";
            }
        }
    }
}