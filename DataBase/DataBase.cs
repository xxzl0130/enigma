﻿using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SQLite;

// 这个文件里放DataBase的基础信息和接口
namespace enigma.DataBase
{
    public partial class DB
    {
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

        /// <summary>
        /// 进入统计结果的数据的最低出现次数
        /// </summary>
        public int FilterCount = 100;

        private SQLiteConnectionWithLock _db;

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
        /// 需要创建Table的类型列表
        /// </summary>
        private static List<Type> _createTableList = new List<Type>()
        {
            typeof(GunDevelop),
            typeof(GunDevelopTotal),
            typeof(GunDevelopHeavy),
            typeof(GunDevelopHeavyTotal),
            typeof(EquipProduce),
            typeof(EquipProduceTotal),
            typeof(EquipDevelop),
            typeof(EquipDevelopTotal),
            typeof(EquipDevelopHeavy),
            typeof(EquipDevelopHeavyTotal),
            typeof(MissionBattle),
            typeof(MissionBattleTotal),
            typeof(MissionFinish),
            typeof(MissionFinishTotal),
            typeof(TimeMark)
        };

        /// <summary>
        /// 启动数据库运行
        /// </summary>
        /// <param name="dataBasePath">数据库路径</param>
        public void Start(string dataBasePath = null)
        {
            if (dataBasePath != null)
                this.DataBasePath = dataBasePath;
            _db = new SQLiteConnectionWithLock(new SQLiteConnectionString(
                DataBasePath,
                SQLiteOpenFlags.Create | SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.FullMutex, true))
            {
                // debug log
                Trace = true,
                TimeExecution = true,
                Tracer = s => Log?.Verbose(" [pid={0}] {1}", Thread.CurrentThread.ManagedThreadId, s),
                BusyTimeout = TimeSpan.FromMilliseconds(5000),
                SkipLock = false
            };

            // 创建表，这个库会自动处理数据结构变更和重复创建
            foreach (var type in _createTableList)
            {
                _db.CreateTable(type);
            }

            updateTableDelegates[DataBaseType.GunDevelop] = new UpdateTableDelegate(UpdateGunDevelopTotal);
            updateTableDelegates[DataBaseType.GunDevelopHeavy] = new UpdateTableDelegate(UpdateGunDevelopHeavyTotal);
            updateTableDelegates[DataBaseType.EquipDevelop] = new UpdateTableDelegate(UpdateEquipDevelopTotal);
            updateTableDelegates[DataBaseType.EquipProduce] = new UpdateTableDelegate(UpdateEquipProduceTotal);
            updateTableDelegates[DataBaseType.EquipDevelopHeavy] = new UpdateTableDelegate(UpdateEquipDevelopHeavyTotal);
            updateTableDelegates[DataBaseType.MissionBattle] = new UpdateTableDelegate(UpdateMissionBattleTotal);
            updateTableDelegates[DataBaseType.MissionFinish] = new UpdateTableDelegate(UpdateMissionFinishTotal);
        }

        public void Stop()
        {
            _db.Close();
            _db = null;
        }

        ~DB()
        {
            Stop();
        }

        /// <summary>
        /// 跟proxy对接的接口，接收json object，解析数据并插入数据库
        /// </summary>
        /// <param name="data">数据</param>
        /// <param name="typename">数据类型名称，JSON里没有的话就要提供</param>
        public void ReceiveDataObject(JObject data, string typename = null)
        {
            object obj = null;
            List<object> objList = null;
            var type = data.ContainsKey("type") ? data.Value<string>("type") : typename;
            Log?.Information("Database receive {type}.", type);
            try
            {
                switch (type)
                {
                    case "Gun/developGun":
                    {
                        // 奇数是普通建造
                        if (data.Value<int>("build_slot") % 2 == 1)
                        {
                            obj = data.ToObject<GunDevelop>();
                        }
                        else
                        {
                            obj = data.ToObject<GunDevelopHeavy>();
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
                                var tmp = data.ToObject<GunDevelop>();
                                tmp.gun_id = id;
                                objList.Add(tmp);
                            }
                            else
                            {
                                var tmp = data.ToObject<GunDevelopHeavy>();
                                tmp.gun_id = id;
                                objList.Add(tmp);
                            }
                        }

                        break;
                    }
                    case "Mission/battleFinish":
                    {
                        var tmp = data.ToObject<MissionBattle>();
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

                        _enemy2mission.TryGetValue(tmp.enemy, out var mission_id);
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

                        _spot2mission.TryGetValue(tmp.spot_id, out var mission_id);
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
                            var tmp = data.ToObject<EquipProduce>();
                            tmp.equip_id = equip.Value<int>();
                            objList.Add(tmp);
                        }

                        break;
                    }
                    case "Equip/develop":
                    {
                        if (data.Value<int>("build_slot") % 2 == 1)
                        {
                            obj = data.ToObject<EquipDevelop>();
                        }
                        else
                        {
                            obj = data.ToObject<EquipDevelopHeavy>();
                        }

                        break;
                    }
                    case "Equip/developMulti":
                    {
                        objList = new List<object>();
                        var equips = data.Value<JArray>("equip_ids");
                        var basic = data.ToObject<EquipDevelopHeavy>(); // 获取基础信息
                        foreach (var equip in equips)
                        {
                            if (equip["slot"].Value<int>() % 2 == 1)
                            {
                                var tmp = equip["info"].ToObject<EquipDevelop>();
                                tmp.timestamp = basic.timestamp;
                                tmp.mp = basic.mp;
                                tmp.ammo = basic.ammo;
                                tmp.mre = basic.mre;
                                tmp.part = basic.part;
                                objList.Add(tmp);
                            }
                            else
                            {
                                var tmp = equip["info"].ToObject<EquipDevelopHeavy>();
                                tmp.timestamp = basic.timestamp;
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
            }
            catch (Exception e)
            {
                Log?.Warning("Error during insert data : {0}", e.ToString());
            }

            if (obj != null)
            {
                Log?.Debug("Database insert {obj}", obj.ToString());
                _db.Insert(obj);
            }

            if (objList != null)
            {
                Log?.Debug("Database insert {n} objects.", objList.Count);
                _db.InsertAll(objList);
            }
        }

        /// <summary>
        /// 批量导入数据
        /// </summary>
        /// <param name="data">JSON格式数据，每个key对应一个type，包含一个array</param>
        public void ImportData(JObject data)
        {
            _db.BeginTransaction();
            foreach (var it in data)
            {
                var count = 0;
                foreach (var obj in it.Value)
                {
                    ReceiveDataObject(obj.Value<JObject>(), it.Key);
                    ++count;
                }

                Log?.Information("Import {n} {type} records.", count, it.Key);
            }

            _db.Commit();
        }

        /// <summary>
        /// 导出一段时间的数据
        /// </summary>
        /// <param name="from">开始时间戳</param>
        /// <param name="to">结束时间戳</param>
        /// <returns>数据</returns>
        public JObject ExportData(int from, int to)
        {
            JObject data = new JObject();

            var list = new List<object>();
            list.AddRange(Query<GunDevelop>(from, to));
            list.AddRange(Query<GunDevelopHeavy>(from, to));
            data["Gun/developGun"] = JsonConvert.SerializeObject(list);
            data["Mission/battleFinish"] = JsonConvert.SerializeObject(Query<MissionBattle>(from, to));
            data["Mission/endTurn"] = JsonConvert.SerializeObject(Query<MissionFinish>(from, to));
            data["Equip/produceDevelop"] = JsonConvert.SerializeObject(Query<EquipProduce>(from, to));

            list = new List<object>();
            list.AddRange(Query<EquipDevelop>(from, to));
            list.AddRange(Query<EquipDevelopHeavy>(from, to));
            data["Equip/develop"] = JsonConvert.SerializeObject(list);

            return data;
        }

        /// <summary>
        /// 导出所有统计数据
        /// </summary>
        public JObject DumpStatisticsData()
        {
            var data = new JObject();
            var statisticsList = new List<Type>()
            {
                typeof(GunDevelopTotal),
                typeof(GunDevelopHeavyTotal),
                typeof(EquipProduceTotal),
                typeof(EquipDevelopTotal),
                typeof(EquipDevelopHeavyTotal),
                typeof(MissionBattleTotal),
                typeof(MissionFinishTotal),
            };
            foreach (var type in statisticsList)
            {
                var mapping = _db.GetMapping(type);
                var resultList = _db.Query(mapping, $"SELECT * FROM '{mapping.TableName}';");
                var obj = new JObject();
                // 这些统计类型都有time_id
                var property = type.GetProperty("time_id");
                foreach (var r in resultList)
                {
                    var t = JsonConvert.SerializeObject(r);
                    obj[property.GetValue(r)] = t;
                }

                data[nameof(type)] = obj;
            }

            return data;
        }

        /// <summary>
        /// 导出所有时间标签
        /// </summary>
        public JObject DumpTimeMarks()
        {
            var data = new JObject();
            var result = _db.Table<TimeMark>().ToList();
            foreach (var timeMark in result)
            {
                data[timeMark.ID] = JsonConvert.SerializeObject(timeMark);
            }

            return data;
        }

        /// <summary>
        /// 插入一个时间标签，如果id已存在或者不为0则是更新
        /// </summary>
        /// <param name="timeMark"></param>
        public void AddTimeMark(TimeMark timeMark)
        {
            if (timeMark.ID == 0)
                _db.Insert(timeMark);
            else
                _db.InsertOrReplace(timeMark);
        }

        /// <summary>
        /// 根据id删除时间标签
        /// </summary>
        /// <param name="id">时间标签的id</param>
        public void DelTimeMark(int id)
        {
            _db.Table<TimeMark>().Delete(mark => mark.ID == id);
        }

        /// <summary>
        /// 查询一段时间内的数据
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <param name="fromUTC">开始时间</param>
        /// <param name="toUTC">结束时间</param>
        /// <returns>结果</returns>
        private List<T> Query<T>(int fromUTC, int toUTC) where T : RecordBase, new()
        {
            Log?.Debug("Query {name} from {from} to {to}.", typeof(T).Name, fromUTC, toUTC);
            return Query<T>(v => v.timestamp >= fromUTC && v.timestamp <= toUTC);
        }

        /// <summary>
        /// 根据自定义规则查询数据
        /// </summary>
        /// <typeparam name="T">数据类型</typeparam>
        /// <param name="expression">规则表达式</param>
        /// <returns>数据结果</returns>
        private List<T> Query<T>(Expression<Func<T, bool>> expression) where T : RecordBase, new()
        {
            return _db.Table<T>().Where(expression).ToList();
        }

        /// <summary>
        /// 备份数据库
        /// </summary>
        /// <param name="backDataBasePath">备份路径</param>
        public void Backup(string backDataBasePath)
        {
            Log?.Information("Backup database to {path}.", backDataBasePath);
            _db.Backup(backDataBasePath);
        }

        /// <summary>
        /// 获取UTC时间戳
        /// </summary>
        /// <returns>UTC时间戳</returns>
        private static int GetUTC()
        {
            return (int) (DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds;
        }

        /// <summary>
        /// 4项资源公式字符串
        /// </summary>
        private const string FormulaStr = "mp,ammo,mre,part";

        /// <summary>
        /// 4项资源加重建等级公式字符串
        /// </summary>
        private const string FormulaLevelStr = FormulaStr + ",input_level";

        /// <summary>
        /// 时间戳列字符串
        /// </summary>
        private const string TimeStr = "timestamp";

        /// <summary>
        /// 单例对象
        /// </summary>
        private static readonly DB _instance = new DB();
    }
}