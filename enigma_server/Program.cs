using System;
using System.Linq;
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

            var rule = Proxy.Instance._ruleJObject["Gun/developGun"];
            var req = (JObject)JsonConvert.DeserializeObject("{\"mp\":30,\"ammo\":30,\"mre\":30,\"part\":30,\"build_slot\":3,\"input_level\":0}");
            var resp = (JObject) JsonConvert.DeserializeObject("{\"gun_id_\":\"9\"}");
            var dataJObject = new JObject();
            while (true)
            {
                var reqRule = rule.Value<JObject>("request");
                foreach (var (s, token) in reqRule)
                {
                    JToken obj = req;
                    // 循环递归查找
                    foreach (var layer in token)
                    {
                        var key = layer.Value<string>();
                        obj = obj[key];
                    }

                    dataJObject[s] = obj;
                }

                break;
            }

            while (true)
            {
                var respRule = rule.Value<JObject>("response");
                foreach (var (s, token) in respRule)
                {
                    JToken obj = resp;
                    // 循环递归查找
                    foreach (var layer in token)
                    {
                        var key = layer.Value<string>();
                        obj = obj[key];
                    }

                    dataJObject[s] = obj;
                }

                break;
            }
            Console.WriteLine(dataJObject);

            Console.ReadKey();
        }

        private static void DataEvent(Newtonsoft.Json.Linq.JObject jsonObject)
        {
            Console.WriteLine(jsonObject.ToString());
        }
    }
}
