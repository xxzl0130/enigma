using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq.Expressions;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SQLite;

// 这个文件里放DataBase的更新统计的接口
namespace enigma.DataBase
{
    public partial class DB
    {
        /// <summary>
        /// 更新普通建造人型统计
        /// </summary>
        /// <param name="timeRanges">时间范围列表</param>
        /// <param name="timeID">时间范围id</param>
        public void UpdateGunDevelopTotal(IEnumerable<TimeRange> timeRanges, int timeID)
        {
            UpdateTable<GunDevelop, GunDevelopTotal>(timeRanges, timeID,
                FormulaStr, "gun_id",
                total => $"mp == {total.mp} AND mre == {total.mre} AND " +
                         $"ammo == {total.ammo} AND part == {total.part} ",
                (obj, total, id, timeId) =>
                {
                    obj.time_id = timeId;
                    obj.valid_total = total;
                    obj.valid_rate = (double) obj.valid_total / obj.total;
                    obj.gun_id = id;
                    return obj;
                },
                total => $"mp == {total.mp} AND mre == {total.mre} AND " +
                         $"ammo == {total.ammo} AND part == {total.part} AND " +
                         $"gun_id == {total.gun_id} AND time_id == {total.time_id}"
            );
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
        /// 更新重型建造人型统计
        /// </summary>
        /// <param name="timeRanges">时间范围列表</param>
        /// <param name="timeID">时间范围id</param>
        public void UpdateGunDevelopHeavyTotal(IEnumerable<TimeRange> timeRanges, int timeID)
        {
            UpdateTable<GunDevelopHeavy, GunDevelopHeavyTotal>(timeRanges, timeID,
                FormulaLevelStr, "gun_id",
                total => $"mp == {total.mp} AND mre == {total.mre} " +
                         $"ammo == {total.ammo} AND part == {total.part} AND input_level == {total.input_level}",
                (obj, total, id, timeId) =>
                {
                    obj.time_id = timeId;
                    obj.valid_total = total;
                    obj.valid_rate = (double) obj.valid_total / obj.total;
                    obj.gun_id = id;
                    return obj;
                },
                total => $"mp == {total.mp} AND mre == {total.mre} AND" +
                         $"ammo == {total.ammo} AND part == {total.part} AND" +
                         $"input_level == {total.input_level} AND gun_id == {total.gun_id} AND" +
                         $"time_id == {total.time_id}"
            );
        }

        /// <summary>
        /// 更新重型建造人型统计
        /// </summary>
        /// <param name="timeRange">时间范围</param>
        /// <param name="timeID">时间范围id</param>
        public void UpdateGunDevelopHeavyTotal(TimeRange timeRange, int timeID)
        {
            UpdateGunDevelopHeavyTotal(new List<TimeRange> {timeRange}, timeID);
        }

        /// <summary>
        /// 更新普通建造装备统计
        /// </summary>
        /// <param name="timeRanges">时间范围列表</param>
        /// <param name="timeID">时间范围id</param>
        public void UpdateEquipDevelopTotal(IEnumerable<TimeRange> timeRanges, int timeID)
        {
            UpdateTable<EquipDevelop, EquipDevelopTotal>(timeRanges, timeID,
                FormulaStr, "equip_id",
                total => $"mp == {total.mp} AND mre == {total.mre} AND " +
                         $"ammo == {total.ammo} AND part == {total.part} ",
                (obj, total, id, timeId) =>
                {
                    obj.time_id = timeId;
                    obj.valid_total = total;
                    obj.valid_rate = (double) obj.valid_total / obj.total;
                    obj.equip_id = id;
                    return obj;
                },
                total => $"mp == {total.mp} AND mre == {total.mre} AND " +
                         $"ammo == {total.ammo} AND part == {total.part} AND " +
                         $"equip_id == {total.equip_id} AND time_id == {total.time_id}"
            );
        }

        /// <summary>
        /// 更新普通建造装备统计
        /// </summary>
        /// <param name="timeRange">时间范围</param>
        /// <param name="timeID">时间范围id</param>
        public void UpdateEquipDevelopTotal(TimeRange timeRange, int timeID)
        {
            UpdateEquipDevelopTotal(new List<TimeRange> {timeRange}, timeID);
        }

        /// <summary>
        /// 更新推荐公式建造装备统计
        /// </summary>
        /// <param name="timeRanges">时间范围列表</param>
        /// <param name="timeID">时间范围id</param>
        public void UpdateEquipProduceTotal(IEnumerable<TimeRange> timeRanges, int timeID)
        {
            UpdateTable<EquipProduce, EquipProduceTotal>(timeRanges, timeID,
                "formula_id", "equip_id",
                total => $"formula_id == {total.formula_id} ",
                (obj, total, id, timeId) =>
                {
                    obj.time_id = timeId;
                    obj.valid_total = total;
                    obj.valid_rate = (double) obj.valid_total / obj.total;
                    obj.equip_id = id;
                    return obj;
                },
                total => $"formula_id == {total.formula_id} AND " +
                         $"equip_id == {total.equip_id} AND time_id == {total.time_id}"
            );
        }

        /// <summary>
        /// 更新推荐公式建造装备统计
        /// </summary>
        /// <param name="timeRange">时间范围</param>
        /// <param name="timeID">时间范围id</param>
        public void UpdateEquipProduceTotal(TimeRange timeRange, int timeID)
        {
            UpdateEquipProduceTotal(new List<TimeRange> {timeRange}, timeID);
        }

        /// <summary>
        /// 更新重型建造装备统计
        /// </summary>
        /// <param name="timeRanges">时间范围列表</param>
        /// <param name="timeID">时间范围id</param>
        public void UpdateEquipDevelopHeavyTotal(IEnumerable<TimeRange> timeRanges, int timeID)
        {
            // 更新出装备的
            UpdateTable<EquipDevelopHeavy, EquipDevelopHeavyTotal>(timeRanges, timeID,
                FormulaLevelStr, "equip_id",
                total => $"mp == {total.mp} AND mre == {total.mre} AND " +
                         $"ammo == {total.ammo} AND part == {total.part} AND " +
                         $"input_level == {total.input_level} ",
                (obj, total, id, timeId) =>
                {
                    obj.time_id = timeId;
                    obj.valid_total = total;
                    obj.valid_rate = (double) obj.valid_total / obj.total;
                    obj.equip_id = id;
                    return obj;
                },
                total => $"mp == {total.mp} AND mre == {total.mre} AND " +
                         $"ammo == {total.ammo} AND part == {total.part} AND " +
                         $"equip_id == {total.equip_id} AND time_id == {total.time_id} AND " +
                         $"input_level == {total.input_level} "
            );
            // 更新出妖精的
            UpdateTable<EquipDevelopHeavy, EquipDevelopHeavyTotal>(timeRanges, timeID,
                FormulaLevelStr, "fairy_id",
                total => $"mp == {total.mp} AND mre == {total.mre} AND " +
                         $"ammo == {total.ammo} AND part == {total.part} AND " +
                         $"input_level == {total.input_level} ",
                (obj, total, id, timeId) =>
                {
                    obj.time_id = timeId;
                    obj.valid_total = total;
                    obj.valid_rate = (double) obj.valid_total / obj.total;
                    obj.fairy_id = id;
                    return obj;
                },
                total => $"mp == {total.mp} AND mre == {total.mre} AND " +
                         $"ammo == {total.ammo} AND part == {total.part} AND " +
                         $"fairy_id == {total.fairy_id} AND time_id == {total.time_id} AND " +
                         $"input_level == {total.input_level} "
            );
        }

        /// <summary>
        /// 更新重型建造装备统计
        /// </summary>
        /// <param name="timeRange">时间范围</param>
        /// <param name="timeID">时间范围id</param>
        public void UpdateEquipDevelopHeavyTotal(TimeRange timeRange, int timeID)
        {
            UpdateEquipDevelopHeavyTotal(new List<TimeRange> {timeRange}, timeID);
        }

        /// <summary>
        /// 更新战斗统计
        /// </summary>
        /// <param name="timeRanges">时间范围</param>
        /// <param name="timeID">时间范围id</param>
        public void UpdateMissionBattleTotal(IEnumerable<TimeRange> timeRanges, int timeID)
        {
            // 不用搜救的枪记录
            UpdateTable<MissionBattle, MissionBattleTotal>(timeRanges, timeID,
                "enemy,battle_rank", "gun_id",
                total => $"enemy == {total.enemy} AND battle_rank == {total.battle_rank} AND " +
                         $"(use_fairy_skill == 0 OR use_fairy_id != {SearchFairyID} OR fairy_skill_lv != 10) ",
                (obj, total, id, timeId) =>
                {
                    obj.time_id = timeId;
                    obj.gun_total = total;
                    obj.gun_id = id;
                    obj.gun_rate = (double) obj.gun_total / obj.total;
                    obj.use_search_fairy = false;
                    return obj;
                },
                total => $"enemy == {total.enemy} AND battle_rank == {total.battle_rank} AND " +
                         $"gun_id == {total.gun_id} AND time_id == {total.time_id} AND use_search_fairy == 0",
                "gun_id_extra"
            );
            // 用搜救的枪记录
            UpdateTable<MissionBattle, MissionBattleTotal>(timeRanges, timeID,
                "enemy,battle_rank", "gun_id",
                total => $"enemy == {total.enemy} AND battle_rank == {total.battle_rank} AND " +
                         $"use_fairy_skill == 1 AND use_fairy_id == {SearchFairyID} AND fairy_skill_lv == 10 ",
                (obj, total, id, timeId) =>
                {
                    obj.time_id = timeId;
                    obj.gun_total = total;
                    obj.gun_id = id;
                    obj.gun_rate = (double) obj.gun_total / obj.total;
                    obj.use_search_fairy = true;
                    return obj;
                },
                total => $"enemy == {total.enemy} AND battle_rank == {total.battle_rank} AND " +
                         $"gun_id == {total.gun_id} AND time_id == {total.time_id} AND use_search_fairy == 1",
                "gun_id_extra"
            );
            // 装备记录，搜救无所谓
            UpdateTable<MissionBattle, MissionBattleTotal>(timeRanges, timeID,
                "enemy,battle_rank", "equip_id",
                total => $"enemy == {total.enemy} AND battle_rank == {total.battle_rank} AND " +
                         $"use_fairy_skill == 1 AND use_fairy_id == {SearchFairyID} AND fairy_skill_lv == 10 ",
                (obj, total, id, timeId) =>
                {
                    obj.time_id = timeId;
                    obj.equip_total = total;
                    obj.equip_id = id;
                    obj.equip_rate = (double) obj.equip_total / obj.total;
                    obj.use_search_fairy = false;
                    return obj;
                },
                total => $"enemy == {total.enemy} AND battle_rank == {total.battle_rank} AND " +
                         $"equip_id == {total.equip_id} AND time_id == {total.time_id}",
                "equip_id_extra"
            );
        }

        /// <summary>
        /// 更新战斗统计
        /// </summary>
        /// <param name="timeRange">时间范围</param>
        /// <param name="timeID">时间范围id</param>
        public void UpdateMissionBattleTotal(TimeRange timeRange, int timeID)
        {
            UpdateMissionBattleTotal(new List<TimeRange> {timeRange}, timeID);
        }

        /// <summary>
        /// 更新战役统计
        /// </summary>
        /// <param name="timeRanges">时间范围</param>
        /// <param name="timeID">时间范围id</param>
        public void UpdateMissionFinishTotal(IEnumerable<TimeRange> timeRanges, int timeID)
        {
            // 枪记录
            UpdateTable<MissionFinish, MissionFinishTotal>(timeRanges, timeID,
                "mission_id,mission_rank", "gun_id",
                total => $"mission_id == {total.mission_id} AND mission_rank == {total.mission_rank} ",
                (obj, total, id, timeId) =>
                {
                    obj.time_id = timeId;
                    obj.gun_total = total;
                    obj.gun_id = id;
                    obj.gun_rate = (double) obj.gun_total / obj.total;
                    return obj;
                },
                total => $"mission_id == {total.mission_id} AND mission_rank == {total.mission_rank} AND " +
                         $"gun_id == {total.gun_id} AND time_id == {total.time_id}",
                "gun_id_extra"
            );
            // 装备记录
            UpdateTable<MissionFinish, MissionFinishTotal>(timeRanges, timeID,
                "mission_id,mission_rank", "equip_id",
                total => $"mission_id == {total.mission_id} AND mission_rank == {total.mission_rank} ",
                (obj, total, id, timeId) =>
                {
                    obj.time_id = timeId;
                    obj.equip_total = total;
                    obj.equip_id = id;
                    obj.equip_rate = (double) obj.equip_total / obj.total;
                    return obj;
                },
                total => $"mission_id == {total.mission_id} AND mission_rank == {total.mission_rank} AND " +
                         $"equip_id == {total.equip_id} AND time_id == {total.time_id}",
                "equip_id_extra"
            );
        }

        /// <summary>
        /// 更新战役统计
        /// </summary>
        /// <param name="timeRange">时间范围</param>
        /// <param name="timeID">时间范围id</param>
        public void UpdateMissionFinishTotal(TimeRange timeRange, int timeID)
        {
            UpdateMissionFinishTotal(new List<TimeRange> {timeRange}, timeID);
        }

        /// <summary>
        /// 更新统计表的代理
        /// </summary>
        /// <param name="timeRanges">时间范围</param>
        /// <param name="timeID">时间范围id</param>
        private delegate void UpdateTableDelegate(IEnumerable<TimeRange> timeRanges, int timeID);

        /// <summary>
        /// 更新统计表的代理字典
        /// </summary>
        private Dictionary<string, UpdateTableDelegate> updateTableDelegates =
            new Dictionary<string, UpdateTableDelegate>();

        /// <summary>
        /// 根据时间标签中的信息更新统计数据，不检查时间合法性
        /// </summary>
        /// <param name="timeMark">时间标签</param>
        public void UpdateTotal(TimeMark timeMark)
        {
            foreach (var type in timeMark.ContainTypes)
            {
                if (updateTableDelegates.TryGetValue(type, out var updateDelegate))
                {
                    updateDelegate(timeMark.TimeRanges, timeMark.ID);
                }
            }
        }

        /// <summary>
        /// 根据时间标签中的信息更新统计数据，不检查时间合法性
        /// </summary>
        /// <param name="timeMarks">时间标签列表</param>
        public void UpdateTotal(IEnumerable<TimeMark> timeMarks)
        {
            foreach (var timeMark in timeMarks)
            {
                UpdateTotal(timeMark);
            }
        }

        /// <summary>
        /// 自动更新所有时间标签下的数据
        /// </summary>
        public void UpdateAll()
        {
            var timeMarks = _db.Table<TimeMark>().ToList();
            foreach (var timeMark in timeMarks)
            {
                var utc = GetUTC();
                // 超过更新时限的不更新
                if (timeMark.UpdateLimitTime > 0 && utc > timeMark.UpdateLimitTime)
                    continue;
                // 设为永不更新的不更新
                if (timeMark.UpdateInterval < 0)
                    continue;
                // 没达到更新间隔的不更新
                if (utc - timeMark.LastUpdateTime < timeMark.UpdateInterval)
                    continue;
                UpdateTotal(timeMark);
            }
        }

        /// <summary>
        /// 根据输入创建公式语句的函数
        /// </summary>
        /// <typeparam name="T">记录/统计信息类型</typeparam>
        /// <param name="obj">数据</param>
        /// <returns>where后参数语句</returns>
        private delegate string MakeFormulaCmd<T>(T obj) where T : RecordBase, new();

        /// <summary>
        /// 更新统计信息的函数
        /// </summary>
        /// <typeparam name="T">统计信息类型</typeparam>
        /// <param name="obj">数据</param>
        /// <param name="total">查询出的总量</param>
        /// <param name="id">id</param>
        /// <param name="timeId">时间id</param>
        /// <returns>更新后的数据</returns>
        private delegate T UpdateCount<T>(T obj, int total, int id, int timeId) where T : RecordBase, new();

        /// <summary>
        /// UpdateTable调用次数统计，用于生产临时表名
        /// </summary>
        private int _updateCallCount = 0;

        private object _updateCallCountLock = new object();

        /// <summary>
        /// 更新统计表的抽象共用函数
        /// </summary>
        /// <typeparam name="TRecordType">单条记录类型</typeparam>
        /// <typeparam name="TCountType">统计数据类型</typeparam>
        /// <param name="timeRanges">时间列表</param>
        /// <param name="timeID">时间id</param>
        /// <param name="groupBy">统计的公式的group by参数</param>
        /// <param name="idName">要统计的id的名称</param>
        /// <param name="idName2">要统计的id的别名</param>
        /// <param name="makeFormulaCmd">创建该公式的临时表的where语句参数</param>
        /// <param name="updateCount">更新统计信息</param>
        /// <param name="makeFindSameCmd">查找是否已有相同条件的统计信息的where语句参数</param>
        private void UpdateTable<TRecordType, TCountType>
        (IEnumerable<TimeRange> timeRanges, int timeID, string groupBy, string idName,
            MakeFormulaCmd<TCountType> makeFormulaCmd, UpdateCount<TCountType> updateCount,
            MakeFormulaCmd<TCountType> makeFindSameCmd, string idName2 = null)
            where TRecordType : RecordBase, new()
            where TCountType : RecordBase, new()
        {
            Log?.Information("Start Update {0} with time ID = {1} in thread pid = {2}.",
                typeof(TCountType).Name,
                timeID, Thread.CurrentThread.ManagedThreadId);
            var watch = new Stopwatch();
            watch.Start();
            try
            {
                var recordTable = _db.GetMapping<TRecordType>();
                var countTable = _db.GetMapping<TCountType>();
                string cmd;
                string tmpTableName;
                string formulaTmpTable;
                lock (_updateCallCountLock)
                {
                    ++_updateCallCount;
                    tmpTableName = "Temp" + typeof(TRecordType).Name + "_" + _updateCallCount;
                    formulaTmpTable = "Temp" + typeof(TRecordType).Name + "Formula" + "_" + _updateCallCount;
                }

                cmd = $"DROP TABLE IF EXISTS '{tmpTableName}';";
                _db.Execute(cmd);
                cmd = $"CREATE TEMP TABLE '{tmpTableName}' " +
                      $"AS SELECT * FROM {recordTable.TableName} " +
                      $"WHERE {TimeRange.TimeRangeList2SQL(timeRanges, TimeStr)} " +
                      $"AND ({groupBy}) in (select {groupBy} FROM " +
                      $"{recordTable.TableName} GROUP BY {groupBy} " +
                      $"HAVING count(*) >= {FilterCount});";
                _db.Execute(cmd); // 创建时间段临时表，同时过滤数量
                // 获取不重复的公式
                cmd = $"SELECT *,count(*) AS total FROM '{tmpTableName}' " +
                      $"GROUP BY {groupBy}";
                var formulaList = _db.Query<TCountType>(cmd);

                // 创建临时表
                _db.Execute($"CREATE TEMP TABLE '{formulaTmpTable}' " +
                            $"AS SELECT * FROM {tmpTableName} LIMIT 1;");
                _db.Execute($"DELETE FROM '{formulaTmpTable}';");

                var updateList = new List<TCountType>();
                var insertList = new List<TCountType>();
                var timestamp = GetUTC();
                foreach (var it in formulaList)
                {
                    //创建该公式的临时表
                    cmd = $"INSERT INTO '{formulaTmpTable}' " +
                          $"SELECT * FROM '{tmpTableName}' WHERE " +
                          makeFormulaCmd(it) + ";";
                    _db.Execute(cmd);
                    it.timestamp = timestamp;
                    // 选取要查找的id（gun/equip等）
                    cmd = $"SELECT DISTINCT {idName} FROM '{formulaTmpTable}'";
                    var idList = _db.QueryScalars<int>(cmd);

                    foreach (var id in idList)
                    {
                        cmd = $"SELECT count(*) FROM {formulaTmpTable} " +
                              $"WHERE {idName} == {id}";
                        if (idName2 != null)
                            cmd += $" AND {idName2} == {id};";
                        else
                            cmd += ";";
                        var total = _db.QueryScalars<int>(cmd);
                        updateCount(it, total[0], id, timeID);
                        // 查询之前是否已经有相同公式的记录
                        cmd = $"SELECT * FROM '{countTable.TableName}' WHERE " +
                              makeFindSameCmd(it) + ";";
                        var list = _db.Query<TCountType>(cmd);
                        it.id = list.Count > 0 ? list[0].id : 0;
                        updateList.Add(Clone(it));
                        if (list.Count > 0)
                        {
                            it.id = list[0].id;
                            updateList.Add(Clone(it));
                        }
                        else
                        {
                            it.id = 0;
                            insertList.Add(Clone(it));
                        }
                    }

                    _db.Execute($"DELETE FROM '{formulaTmpTable}';");
                }

                //统一更新
                using (_db.Lock())
                {
                    _db.BeginTransaction();
                    foreach (var obj in updateList)
                    {
                        _db.InsertOrReplace(obj);
                    }

                    foreach (var obj in insertList)
                    {
                        _db.Insert(obj);
                    }

                    _db.Execute($"DROP TABLE '{formulaTmpTable}';");
                    _db.Execute($"DROP TABLE '{tmpTableName}';");
                    _db.Commit();
                }
            }
            catch (SQLiteException e)
            {
                Log?.Warning(e, "Error during Update {0} with time ID = {1} in thread pid = {2}, result = {3}.",
                    typeof(TCountType).Name,
                    timeID, Thread.CurrentThread.ManagedThreadId,
                    ((SQLiteException) e).Result);
            }
            catch (Exception e)
            {
                Log?.Warning(e, "Error during Update {0} with time ID = {1} in thread pid = {2}.",
                    typeof(TCountType).Name,
                    timeID, Thread.CurrentThread.ManagedThreadId);
            }

            watch.Stop();
            Log?.Information("Finish Update {0} with time ID = {1} in thread pid = {2} in {3}s .",
                typeof(TCountType).Name,
                timeID, Thread.CurrentThread.ManagedThreadId,
                watch.Elapsed.TotalSeconds);
        }

        /// <summary>
        /// 搜救妖精id
        /// </summary>
        private const int SearchFairyID = 16;

        /// <summary>
        /// 深度拷贝
        /// </summary>
        private static T Clone<T>(T obj)
        {
            var memoryStream = new MemoryStream();
            var formatter = new BinaryFormatter();
            formatter.Serialize(memoryStream, obj);
            memoryStream.Position = 0;
            return (T) formatter.Deserialize(memoryStream);
        }
    }
}