using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SQLite;

namespace enigma
{
    namespace DataBase
    {
        public class DB
        {
            /// <summary>
            /// 单例对象
            /// </summary>
            private static readonly DB _instance = new DB();

            /// <summary>
            /// 获取单例对象
            /// </summary>
            public static DB Instance => _instance;
            
            /// <summary>
            /// 日志接口，由外部提供实例
            /// </summary>
            public Serilog.ILogger Log = null;

            /// <summary>
            /// 数据库路径，需要在启动前提供或者在start函数中提供
            /// </summary>
            public string DataBasePath = "data.db";

            private SQLiteConnection _db;

            /// <summary>
            /// spot_id到mission_id的映射，由提取好的数据库导入
            /// </summary>
            private static Dictionary<int, int> _spot2mission =
                JsonConvert.DeserializeObject<Dictionary<int, int>>(
                    System.Text.Encoding.UTF8.GetString(Resource.spot2mission));

            /// <summary>
            /// enemy_id到mission_id的映射，由提取好的数据库导入
            /// </summary>
            private static Dictionary<int, int> _enemy2mission = 
                JsonConvert.DeserializeObject<Dictionary<int, int>>(
                System.Text.Encoding.UTF8.GetString(Resource.enemy2mission));

            /// <summary>
            /// 启动数据库运行
            /// </summary>
            /// <param name="dataBasePath">数据库路径</param>
            public void Start(string dataBasePath = null)
            {
                if (dataBasePath != null)
                    this.DataBasePath = dataBasePath;
                _db = new SQLiteConnection(DataBasePath);

                // 创建表，这个库会自动处理数据结构变更和重复创建
                _db.CreateTable<DevelopGun>();
                _db.CreateTable<DevelopHeavyGun>();
                _db.CreateTable<ProduceEquip>();
                _db.CreateTable<DevelopEquip>();
                _db.CreateTable<DevelopHeavyEquip>();
                _db.CreateTable<BattleFinish>();
                _db.CreateTable<MissionFinish>();
            }

            /// <summary>
            /// 跟proxy对接的接口，接收json object，解析数据并插入数据库
            /// </summary>
            /// <param name="data">数据</param>
            public void ReceiveDataObject(JObject data)
            {
                object obj = null;
                List<object> objList = null;
                var type = data.Value<string>("type");
                Log?.Information("Database receive {type}.", type);
                switch (type)
                {
                    case "Gun/developGun":
                    {
                        // 奇数是普通建造
                        if (data.Value<int>("build_slot") % 2 == 1)
                        {
                            obj = data.ToObject<DevelopGun>();
                        }
                        else
                        {
                            obj = data.ToObject<DevelopHeavyGun>();
                        }

                        break;
                    }
                    case "Gun/developMultiGun":
                    {
                        var guns = data["gun_ids"];
                        objList = new List<object>();
                        foreach (var gun in guns)
                        {
                            var id = gun.Value<int>("id");
                            var slot = gun.Value<int>("slot");
                            if (slot % 2 == 1)
                            {
                                var tmp = data.ToObject<DevelopGun>();
                                tmp.gun_id = id;
                                objList.Add(tmp);
                            }
                            else
                            {
                                var tmp = data.ToObject<DevelopHeavyGun>();
                                tmp.gun_id = id;
                                objList.Add(tmp);
                            }
                        }
                        break;
                    }
                    case "Mission/battleFinish":
                    {
                        var tmp = data.ToObject<BattleFinish>();
                        if (data.ContainsKey("battle_get_gun"))
                        {
                            var guns = data.Value<JArray>("battle_get_gun");
                            if (guns.Count > 0)
                            {
                                tmp.gun_id = guns[0].Value<int>();
                            }
                            if (guns.Count > 1)
                            {
                                tmp.gun_id_extra = guns[1].Value<int>();
                            }
                        }
                        if (data.ContainsKey("battle_get_equip"))
                        {
                            var equips = data.Value<JArray>("battle_get_equip");
                            if (equips.Count > 0)
                            {
                                tmp.equip_id = equips[0].Value<int>();
                            }
                            if (equips.Count > 1)
                            {
                                tmp.equip_id_extra = equips[1].Value<int>();
                            }
                        }

                        int mission_id = 0;
                        _enemy2mission.TryGetValue(tmp.enemy, out mission_id);
                        tmp.mission_id = mission_id;
                        obj = tmp;
                        break;
                    }
                    case "Mission/endTurn":
                    {
                        var tmp = data.ToObject<MissionFinish>();
                        if (data.ContainsKey("reward_equip"))
                        {
                            var equips = data.Value<JArray>("reward_equip");
                            if (equips.Count > 0)
                            {
                                tmp.equip_id = equips[0].Value<int>();
                            }
                            if (equips.Count > 1)
                            {
                                tmp.equip_id_extra = equips[1].Value<int>();
                            }
                        }
                        if (data.ContainsKey("reward_gun"))
                        {
                            var guns = data.Value<JArray>("reward_gun");
                            if (guns.Count > 0)
                            {
                                tmp.gun_id = guns[0].Value<int>();
                            }
                            if (guns.Count > 1)
                            {
                                tmp.gun_id_extra = guns[1].Value<int>();
                            }
                        }

                        int mission_id = 0;
                        _spot2mission.TryGetValue(tmp.spot_id, out mission_id);
                        tmp.mission_id = mission_id;
                        obj = tmp;
                        break;
                    }
                    case "Equip/produceDevelop":
                    {
                        objList = new List<object>();
                        var equips = data.Value<JArray>("equips");
                        foreach (var equip in equips)
                        {
                            var tmp = data.ToObject<ProduceEquip>();
                            tmp.equip_id = equip.Value<int>();
                            objList.Add(tmp);
                        }
                        break;
                    }
                    case "Equip/develop":
                    {
                        if (data.Value<int>("build_slot") % 2 == 1)
                        {
                            obj = data.ToObject<DevelopEquip>();
                        }
                        else
                        {
                            obj = data.ToObject<DevelopHeavyEquip>();
                        }
                        break;
                    }
                    case "Equip/developMulti":
                    {
                        objList = new List<object>();
                        var equips = data.Value<JArray>("equip_ids");
                        var basic = data.ToObject<DevelopHeavyEquip>(); // 获取基础信息
                        foreach (var equip in equips)
                        {
                            if (equip["slot"].Value<int>() % 2 == 1)
                            {
                                var tmp = equip["info"].ToObject<DevelopEquip>();
                                tmp.timestamp = basic.timestamp;
                                tmp.uid = basic.uid;
                                tmp.mp = basic.mp;
                                tmp.ammo = basic.ammo;
                                tmp.mre = basic.mre;
                                tmp.part = basic.part;
                                objList.Add(tmp);
                            }
                            else
                            {
                                var tmp = equip["info"].ToObject<DevelopHeavyEquip>();
                                tmp.timestamp = basic.timestamp;
                                tmp.uid = basic.uid;
                                tmp.mp = basic.mp;
                                tmp.ammo = basic.ammo;
                                tmp.mre = basic.mre;
                                tmp.part = basic.part;
                                tmp.input_level = basic.input_level;
                                objList.Add(tmp);
                            }
                        }
                        break;
                    }
                }

                if (obj != null)
                {
                    Log?.Information("Database insert {obj}", obj.ToString());
                    _db.Insert(obj);
                }

                if (objList != null)
                {
                    Log?.Information("Database insert {n} objects.", objList.Count);
                    _db.InsertAll(objList);
                }
            }

            /// <summary>
            /// 跟proxy对接的接口的异步版，接收json object，解析数据并插入数据库
            /// </summary>
            /// <param name="data">数据</param>
            /// <returns>task</returns>
            public Task ReceiveDataObjectAsync(JObject data)
            {
                return Task.Factory.StartNew(() => { ReceiveDataObject(data); });
            }
        }
    }
}