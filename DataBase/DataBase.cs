using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
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

            /// <summary>
            /// 进入统计结果的数据的最低出现次数
            /// </summary>
            public int FilterCount = 100;

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
                _db.CreateTable<GunDevelop>();
                _db.CreateTable<GunDevelopTotal>();
                _db.CreateTable<GunDevelopHeavy>();
                _db.CreateTable<GunDevelopHeavyTotal>();
                _db.CreateTable<EquipProduce>();
                _db.CreateTable<EquipProduceTotal>();
                _db.CreateTable<EquipDevelop>();
                _db.CreateTable<EquipDevelopTotal>();
                _db.CreateTable<EquipDevelopHeavy>();
                _db.CreateTable<EquipDevelopHeavyTotal>();
                _db.CreateTable<MissionBattle>();
                _db.CreateTable<MissionBattleTotal>();
                _db.CreateTable<MissionFinish>();
                _db.CreateTable<MissionFinishTotal>();

                // debug log
                _db.Trace = true;
                _db.TimeExecution = true;
                _db.Tracer = s => Log?.Verbose(s);
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

            /// <summary>
            /// 批量导入数据
            /// </summary>
            /// <param name="data">JSON格式数据，每个key对应一个type，包含一个array</param>
            public void ImportData(JObject data)
            {
                foreach (var it in data)
                {
                    _db.BeginTransaction();
                    foreach (var obj in it.Value)
                    {
                        ReceiveDataObject(obj.Value<JObject>(),it.Key);
                    }
                    _db.Commit();
                }
            }

            /// <summary>
            /// 批量导入数据
            /// </summary>
            /// <param name="data">JSON格式数据，每个key对应一个type，包含一个array</param>
            public Task ImportDataAsync(JObject data)
            {
                return Task.Factory.StartNew(() => ImportData(data));
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
            /// 导出一段时间的数据
            /// </summary>
            /// <param name="from">开始时间戳</param>
            /// <param name="to">结束时间戳</param>
            /// <returns>数据</returns>
            public Task<JObject> ExportDataAsync(int from, int to)
            {
                return Task<JObject>.Factory.StartNew(() => ExportData(from, to));
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
            /// 查询一段时间内的数据，异步
            /// </summary>
            /// <typeparam name="T">类型</typeparam>
            /// <param name="fromUTC">开始时间</param>
            /// <param name="toUTC">结束时间</param>
            /// <returns>结果</returns>
            private Task<List<T>> QueryAsync<T>(int fromUTC, int toUTC) where T : RecordBase, new()
            {
                return Task<List<T>>.Factory.StartNew(() => Query<T>(fromUTC, toUTC));
            }

            /// <summary>
            /// 根据自定义规则查询数据
            /// </summary>
            /// <typeparam name="T">数据类型</typeparam>
            /// <param name="expression">规则表达式</param>
            /// <returns>数据结果</returns>
            private List<T> Query<T>(Expression<Func<T,bool>> expression) where T : RecordBase, new()
            {
                return _db.Table<T>().Where(expression).ToList();
            }

            /// <summary>
            /// 根据自定义规则查询数据，异步
            /// </summary>
            /// <typeparam name="T">数据类型</typeparam>
            /// <param name="expression">规则表达式</param>
            /// <returns>数据结果</returns>
            private Task<List<T>> QueryAsync<T>(Expression<Func<T, bool>> expression) where T : RecordBase, new()
            {
                return Task<List<T>>.Factory.StartNew(() => Query<T>(expression));
            }

            /// <summary>
            /// 备份数据库
            /// </summary>
            /// <param name="backDataBasePath">备份路径</param>
            public void Backup(string backDataBasePath)
            {
                Log?.Information("Backup database to {path}.", backDataBasePath);
                _db.Backup(backDataBasePath);
                Log?.Information("Database backup complete.");
            }

            /// <summary>
            /// 备份数据库异步
            /// </summary>
            /// <param name="backDataBasePath">备份路径</param>
            public Task BackupAsync(string backDataBasePath)
            {
                return Task.Factory.StartNew(() => { Backup(backDataBasePath); });
            }

            /// <summary>
            /// 更新普通建造人型统计
            /// </summary>
            /// <param name="timeRanges">时间范围列表</param>
            /// <param name="timeID">时间范围id</param>
            public void UpdateGunDevelopTotal(IEnumerable<TimeRange> timeRanges, int timeID)
            {
                Log?.Information("Start UpdateGunDevelopTotal with time ID = {0}.", timeID);
                var gunTable = _db.GetMapping<GunDevelop>();
                var gunTotalTable = _db.GetMapping<GunDevelopTotal>();
                string cmd;
                const string tmpTableName = "TempGunDevelop";
                const string formulaTmpTable = "TempFormula";

                cmd = $"DROP TABLE IF EXISTS {tmpTableName};";
                _db.Execute(cmd);
                cmd = $"CREATE TEMP TABLE {tmpTableName} " +
                      $"AS SELECT * FROM {gunTable.TableName} " +
                      $"WHERE {TimeRange.TimeRangeList2SQL(timeRanges,TimeStr)} " +
                      $"AND ({FormulaStr}) in (select {FormulaStr} FROM " + 
                      $"{gunTable.TableName} GROUP BY {FormulaStr} " +
                      $"HAVING count(*) >= {FilterCount});";
                _db.Execute(cmd); // 创建时间段临时表，同时过滤数量
                // 获取不重复且数量大于过滤下限的公式
                cmd = $"SELECT DISTINCT *,count(*) AS total FROM {tmpTableName} " +
                      $"GROUP BY {FormulaStr} HAVING count(*) >= {FilterCount}";
                var formulaList = _db.Query<GunDevelopTotal>(cmd);
                
                foreach (var it in formulaList)
                {
                    cmd = $"DROP TABLE IF EXISTS {formulaTmpTable};";
                    _db.Execute(cmd);
                    cmd = $"CREATE TEMP TABLE {formulaTmpTable} " +
                          $"AS SELECT * FROM {tmpTableName} " +
                          $"WHERE mp == {it.mp} AND ammo == {it.ammo} AND mre == {it.mre} AND part == {it.part};";
                    _db.Execute(cmd);//创建该公式的临时表
                    
                    cmd = $"SELECT DISTINCT gun_id FROM {formulaTmpTable}";
                    // 获取不重复的gun_id列表
                    var gunList = _db.QueryScalars<int>(cmd);

                    it.time_id = timeID;
                    it.timestamp = GetUTC();
                    foreach (var gun_id in gunList)
                    {
                        cmd = $"SELECT count(*) FROM {formulaTmpTable} " +
                              $"WHERE gun_id = {gun_id};";
                        it.valid_total = _db.ExecuteScalar<int>(cmd);
                        it.valid_rate = (double)it.valid_total / it.total;
                        var last = _db.Query<GunDevelopTotal>(
                            $"SELECT * FROM {gunTotalTable.TableName} WHERE mp == {it.mp} AND ammo == {it.ammo} AND mre == {it.mre} AND part == {it.part} AND gun_id = {gun_id};");
                        it.gun_id = gun_id;
                        if (last.Count > 0)
                        {
                            it.id = last[0].id;
                            _db.InsertOrReplace(it);
                        }
                        else
                        {
                            _db.Insert(it);
                        }
                    }
                    _db.Execute($"DROP table {formulaTmpTable};");
                }

                _db.Execute($"DROP TABLE {tmpTableName};");
            }

            /// <summary>
            /// 更新普通建造人型统计
            /// </summary>
            /// <param name="timeRange">时间范围</param>
            /// <param name="timeID">时间范围id</param>
            public void UpdateGunDevelopTotal(TimeRange timeRange, int timeID)
            {
                UpdateGunDevelopTotal(new List<TimeRange> {timeRange}, timeID);
            }

            /// <summary>
            /// 获取UTC时间戳
            /// </summary>
            /// <returns>UTC时间戳</returns>
            private static int GetUTC()
            {
                return (int)(DateTime.Now - new DateTime(1970, 1, 1)).TotalSeconds;
            }

            /// <summary>
            /// 4项资源公式字符串
            /// </summary>
            private const string FormulaStr = "mp,ammo,mre,part";
            /// <summary>
            /// 时间戳列字符串
            /// </summary>
            private const string TimeStr = "timestamp";
        }
    }
}