using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SQLite;

// 这个文件里放DataBase的异步接口包装
namespace enigma
{
    namespace DataBase
    {
        public partial class DB
        {

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
            public Task UpdateGunDevelopTotalAsync(IEnumerable<TimeRange> timeRanges, int timeID)
            {
                return Task.Factory.StartNew(() => { UpdateGunDevelopTotal(timeRanges, timeID); });
            }

            /// <summary>
            /// 更新普通建造人型统计
            /// </summary>
            /// <param name="timeRange">时间范围</param>
            /// <param name="timeID">时间范围id</param>
            public Task UpdateGunDevelopTotalAsync(TimeRange timeRange, int timeID)
            {
                return Task.Factory.StartNew(() => { UpdateGunDevelopTotal(timeRange, timeID); });
            }

            /// <summary>
            /// 更新重型建造人型统计
            /// </summary>
            /// <param name="timeRanges">时间范围列表</param>
            /// <param name="timeID">时间范围id</param>
            public Task UpdateGunDevelopHeavyTotalAsync(IEnumerable<TimeRange> timeRanges, int timeID)
            {
                return Task.Factory.StartNew(() => { UpdateGunDevelopHeavyTotal(timeRanges, timeID); });
            }

            /// <summary>
            /// 更新重型建造人型统计
            /// </summary>
            /// <param name="timeRange">时间范围</param>
            /// <param name="timeID">时间范围id</param>
            public Task UpdateGunDevelopHeavyTotalAsync(TimeRange timeRange, int timeID)
            {
                return Task.Factory.StartNew(() => { UpdateGunDevelopHeavyTotal(timeRange, timeID); });
            }

            /// <summary>
            /// 更新普通建造装备统计
            /// </summary>
            /// <param name="timeRanges">时间范围列表</param>
            /// <param name="timeID">时间范围id</param>
            public Task UpdateEquipDevelopTotalAsync(IEnumerable<TimeRange> timeRanges, int timeID)
            {
                return Task.Factory.StartNew(() => { UpdateEquipDevelopTotal(timeRanges, timeID); });
            }

            /// <summary>
            /// 更新普通建造装备统计
            /// </summary>
            /// <param name="timeRange">时间范围</param>
            /// <param name="timeID">时间范围id</param>
            public Task UpdateEquipDevelopTotalAsync(TimeRange timeRange, int timeID)
            {
                return Task.Factory.StartNew(() => { UpdateEquipDevelopTotal(timeRange, timeID); });
            }

            /// <summary>
            /// 更新重型建造装备统计
            /// </summary>
            /// <param name="timeRanges">时间范围列表</param>
            /// <param name="timeID">时间范围id</param>
            public Task UpdateEquipDevelopHeavyTotalAsync(IEnumerable<TimeRange> timeRanges, int timeID)
            {
                return Task.Factory.StartNew(() => { UpdateEquipDevelopHeavyTotal(timeRanges, timeID); });
            }

            /// <summary>
            /// 更新重型建造装备统计
            /// </summary>
            /// <param name="timeRange">时间范围</param>
            /// <param name="timeID">时间范围id</param>
            public Task UpdateEquipDevelopHeavyTotalAsync(TimeRange timeRange, int timeID)
            {
                return Task.Factory.StartNew(() => { UpdateEquipDevelopHeavyTotal(timeRange, timeID); });
            }

            /// <summary>
            /// 更新推荐公式建造装备统计
            /// </summary>
            /// <param name="timeRanges">时间范围列表</param>
            /// <param name="timeID">时间范围id</param>
            public Task UpdateEquipProduceTotalAsync(IEnumerable<TimeRange> timeRanges, int timeID)
            {
                return Task.Factory.StartNew(() => { UpdateEquipProduceTotal(timeRanges, timeID); });
            }

            /// <summary>
            /// 更新推荐公式建造装备统计
            /// </summary>
            /// <param name="timeRange">时间范围</param>
            /// <param name="timeID">时间范围id</param>
            public Task UpdateEquipProduceTotalAsync(TimeRange timeRange, int timeID)
            {
                return Task.Factory.StartNew(() => { UpdateEquipProduceTotal(timeRange, timeID); });
            }

            /// <summary>
            /// 更新战斗统计
            /// </summary>
            /// <param name="timeRanges">时间范围列表</param>
            /// <param name="timeID">时间范围id</param>
            public Task UpdateMissionBattleTotalAsync(IEnumerable<TimeRange> timeRanges, int timeID)
            {
                return Task.Factory.StartNew(() => { UpdateMissionBattleTotal(timeRanges, timeID); });
            }

            /// <summary>
            /// 更新战斗统计
            /// </summary>
            /// <param name="timeRange">时间范围</param>
            /// <param name="timeID">时间范围id</param>
            public Task UpdateMissionBattleTotalAsync(TimeRange timeRange, int timeID)
            {
                return Task.Factory.StartNew(() => { UpdateMissionBattleTotal(timeRange, timeID); });
            }

            /// <summary>
            /// 更新战役统计
            /// </summary>
            /// <param name="timeRanges">时间范围列表</param>
            /// <param name="timeID">时间范围id</param>
            public Task UpdateMissionFinishTotalAsync(IEnumerable<TimeRange> timeRanges, int timeID)
            {
                return Task.Factory.StartNew(() => { UpdateMissionFinishTotal(timeRanges, timeID); });
            }

            /// <summary>
            /// 更新战役统计
            /// </summary>
            /// <param name="timeRange">时间范围</param>
            /// <param name="timeID">时间范围id</param>
            public Task UpdateMissionFinishTotalAsync(TimeRange timeRange, int timeID)
            {
                return Task.Factory.StartNew(() => { UpdateMissionFinishTotal(timeRange, timeID); });
            }
        }
    }
}