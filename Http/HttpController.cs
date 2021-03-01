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
            [Route(HttpVerbs.Post, "/data")]
            public bool Login([FormField] string username, [FormField] string password)
            {
                if (username == HttpServer.Instance.Options.AdminUser &&
                    password == HttpServer.Instance.Options.AdminPassword)
                {
                    return true;
                }

                return false;
            }
        }
    }
}