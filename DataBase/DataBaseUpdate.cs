using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SQLite;

// 这个文件里放DataBase的更新统计的接口
namespace enigma
{
    namespace DataBase
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
                Log?.Information("Start UpdateGunDevelopTotal with time ID = {0}.", timeID);
                var gunTable = _db.GetMapping<GunDevelop>();
                var gunTotalTable = _db.GetMapping<GunDevelopTotal>();
                string cmd;
                const string tmpTableName = "TempGunDevelop";
                const string formulaTmpTable = "TempGunDevelopFormula";

                cmd = $"DROP TABLE IF EXISTS {tmpTableName};";
                _db.Execute(cmd);
                cmd = $"CREATE TEMP TABLE {tmpTableName} " +
                      $"AS SELECT * FROM {gunTable.TableName} " +
                      $"WHERE {TimeRange.TimeRangeList2SQL(timeRanges, TimeStr)} " +
                      $"AND ({FormulaStr}) in (select {FormulaStr} FROM " +
                      $"{gunTable.TableName} GROUP BY {FormulaStr} " +
                      $"HAVING count(*) >= {FilterCount});";
                _db.Execute(cmd); // 创建时间段临时表，同时过滤数量
                // 获取不重复的公式
                cmd = $"SELECT DISTINCT *,count(*) AS total FROM {tmpTableName};";
                var formulaList = _db.Query<GunDevelopTotal>(cmd);

                foreach (var it in formulaList)
                {
                    cmd = $"DROP TABLE IF EXISTS {formulaTmpTable};";
                    _db.Execute(cmd);
                    cmd = $"CREATE TEMP TABLE {formulaTmpTable} " +
                          $"AS SELECT * FROM {tmpTableName} " +
                          $"WHERE mp == {it.mp} AND ammo == {it.ammo} AND mre == {it.mre} AND part == {it.part};";
                    _db.Execute(cmd); //创建该公式的临时表

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
                        it.valid_rate = (double) it.valid_total / it.total;
                        // 查询之前是否已经有相同公式的记录
                        var list = _db.Table<GunDevelopTotal>().Where(v =>
                                v.time_id == it.time_id && v.mp == it.mp && v.ammo == it.ammo && v.mre == it.mre &&
                                v.part == it.part)
                            .ToList();
                        it.gun_id = gun_id;
                        if (list.Count > 0)
                        {
                            it.id = list[0].id;
                            _db.InsertOrReplace(it);
                        }
                        else
                        {
                            it.id = 0;
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
            /// 更新重型建造人型统计
            /// </summary>
            /// <param name="timeRanges">时间范围列表</param>
            /// <param name="timeID">时间范围id</param>
            public void UpdateGunDevelopHeavyTotal(IEnumerable<TimeRange> timeRanges, int timeID)
            {
                Log?.Information("Start UpdateGunDevelopHeavyTotal with time ID = {0}.", timeID);
                var gunTable = _db.GetMapping<GunDevelopHeavy>();
                var gunTotalTable = _db.GetMapping<GunDevelopHeavyTotal>();
                string cmd;
                const string tmpTableName = "TempGunDevelopHeavy";
                const string formulaTmpTable = "TempGunDevelopHeavyFormula";

                cmd = $"DROP TABLE IF EXISTS {tmpTableName};";
                _db.Execute(cmd);
                cmd = $"CREATE TEMP TABLE {tmpTableName} " +
                      $"AS SELECT * FROM {gunTable.TableName} " +
                      $"WHERE {TimeRange.TimeRangeList2SQL(timeRanges, TimeStr)} " +
                      $"AND ({FormulaLevelStr}) in (select {FormulaLevelStr} FROM " +
                      $"{gunTable.TableName} GROUP BY {FormulaLevelStr} " +
                      $"HAVING count(*) >= {FilterCount});";
                _db.Execute(cmd); // 创建时间段临时表，同时过滤数量
                // 获取不重复的公式
                cmd = $"SELECT DISTINCT *,count(*) AS total FROM {tmpTableName};";
                var formulaList = _db.Query<GunDevelopHeavyTotal>(cmd);

                foreach (var it in formulaList)
                {
                    cmd = $"DROP TABLE IF EXISTS {formulaTmpTable};";
                    _db.Execute(cmd);
                    cmd = $"CREATE TEMP TABLE {formulaTmpTable} " +
                          $"AS SELECT * FROM {tmpTableName} " +
                          $"WHERE mp == {it.mp} AND ammo == {it.ammo} " +
                          $"AND mre == {it.mre} AND part == {it.part} " +
                          $"AND input_level == {it.input_level}";
                    _db.Execute(cmd); //创建该公式的临时表

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
                        it.valid_rate = (double) it.valid_total / it.total;
                        it.gun_id = gun_id;
                        // 查询之前是否已经有相同公式的记录
                        var list = _db.Table<GunDevelopHeavyTotal>().Where(v =>
                                v.time_id == it.time_id && v.mp == it.mp && v.ammo == it.ammo && v.mre == it.mre
                                && v.part == it.part && v.input_level == it.input_level)
                            .ToList();
                        if (list.Count > 0)
                        {
                            it.id = list[0].id;
                            _db.InsertOrReplace(it);
                        }
                        else
                        {
                            it.id = 0;
                            _db.Insert(it);
                        }
                    }

                    _db.Execute($"DROP table {formulaTmpTable};");
                }

                _db.Execute($"DROP TABLE {tmpTableName};");
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
        }
    }
}