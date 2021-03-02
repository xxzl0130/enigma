using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Web;
using enigma;
using enigma.proxy;
using enigma.DataBase;
using enigma.Http;
using GF_CipherSharp;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog;

namespace enigma_server
{
    class Program
    {
        static void Main(string[] args)
        {
            var Log = new LoggerConfiguration()
                .MinimumLevel.Debug()
                //.MinimumLevel.Verbose()
                .WriteTo.Console()
                //.WriteTo.File("./log.txt")
                .CreateLogger();
            
            try
            {
                Proxy.Instance.Port = 18888;
                Proxy.Instance.DataEvent += DataEvent;
                Proxy.Instance.EnableBlocking = false;
                Proxy.Instance.Log = Log;
                Proxy.Instance.EnableBlocking = true;
                Proxy.Instance.Start();
                DB.Instance.DataBasePath = "test.db";
                DB.Instance.Log = Log;
                DB.Instance.FilterCount = 1;
                DB.Instance.Start();
                HttpServer.Instance.Start(new HttpServer.HttpOptions(){Port = 8877,CorsHeaders = "*"});
            }
            catch (Exception e)
            {
                Log.Warning(e.ToString());
            }

            Console.WriteLine(Proxy.Instance.LocalIPAddress + ":" + Proxy.Instance.Port);

            Console.ReadKey();

            DB.Instance.Stop();
        }

        private static void DataEvent(Newtonsoft.Json.Linq.JObject jsonObject)
        {
            DB.Instance.ReceiveDataObject(jsonObject);
        }
    }
}
