using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Web;
using enigma.proxy;
using enigma.DataBase;
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
            Proxy.Instance.Port = 18888;
            Proxy.Instance.DataEvent += DataEvent;
            Proxy.Instance.EnableBlocking = false;
            Proxy.Instance.Log = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console()
                .CreateLogger();
            Proxy.Instance.Start();
            DB.Instance.DataBasePath = "test.db";
            DB.Instance.Log = Proxy.Instance.Log;
            Stopwatch stw = new Stopwatch();
            stw.Start();
            DB.Instance.Start();
            stw.Stop();
            Console.WriteLine(stw.ElapsedMilliseconds);

            Console.WriteLine(Proxy.Instance.LocalIPAddress + ":" + Proxy.Instance.Port);

            Console.ReadKey();
        }

        private static void DataEvent(Newtonsoft.Json.Linq.JObject jsonObject)
        {
            DB.Instance.ReceiveDataObjectAsync(jsonObject);
        }
    }
}
