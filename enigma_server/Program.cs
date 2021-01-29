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
            var Log = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console()
                .CreateLogger();
            using (var db = new SQLite.SQLiteConnection("test.db"))
            {
                var stw = new Stopwatch();
                stw.Start();
                db.DropTable<GunDevelop>();
                db.DropTable<GunDevelopTotal>();
                db.CreateTable<GunDevelop>();
                var rd = new Random();
                var gun = new GunDevelop();
                for (var j = 0; j < 10; ++j)
                {
                    db.BeginTransaction();
                    for (var i = 0; i < 1000; ++i)
                    {
                        gun.part = 30 + rd.Next(0, 5);
                        gun.ammo = 30 + rd.Next(0, 5);
                        gun.mp = 30 + rd.Next(0, 5);
                        gun.mre = 30 + rd.Next(0, 5);
                        gun.gun_id = rd.Next(1, 20);
                        gun.timestamp = Utils.GetUTC() + rd.Next(-100, 100);
                        db.Insert(gun);
                    }
                    db.Commit();
                }
                stw.Stop();
                Log.Information("生成数据完成，耗时{0}s", stw.Elapsed.TotalSeconds);
            }
            Proxy.Instance.Port = 18888;
            Proxy.Instance.DataEvent += DataEvent;
            Proxy.Instance.EnableBlocking = false;
            Proxy.Instance.Log = Log;
            Proxy.Instance.Start();
            DB.Instance.DataBasePath = "test.db";
            DB.Instance.Log = Log;
            DB.Instance.FilterCount = 10;
            DB.Instance.Start();

            var timer = new Stopwatch();
            timer.Start();
            DB.Instance.UpdateGunDevelopTotal(Utils.GetUTC() - 20,Utils.GetUTC() + 20);
            timer.Stop();
            Log.Information("更新数据完成，耗时{0}s", timer.Elapsed.TotalSeconds);

            Console.WriteLine(Proxy.Instance.LocalIPAddress + ":" + Proxy.Instance.Port);

            Console.ReadKey();
        }

        private static void DataEvent(Newtonsoft.Json.Linq.JObject jsonObject)
        {
            DB.Instance.ReceiveDataObjectAsync(jsonObject);
        }
    }
}
