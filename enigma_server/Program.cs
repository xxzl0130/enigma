using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Web;
using enigma.proxy;
using GF_CipherSharp;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace enigma_server
{
    class Program
    {
        static void Main(string[] args)
        {
            Proxy.Instance.Port = 18888;
            Proxy.Instance.DataEvent += DataEvent; ;
            Proxy.Instance.EnableBlocking = false;
            Proxy.Instance.Start();

            Console.WriteLine(Proxy.Instance.LocalIPAddress + ":" + Proxy.Instance.Port);

            Console.ReadKey();
        }

        private static void DataEvent(Newtonsoft.Json.Linq.JObject jsonObject)
        {
            Console.WriteLine(jsonObject.ToString());
        }
    }
}
