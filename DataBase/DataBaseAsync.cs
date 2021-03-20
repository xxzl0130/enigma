using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SQLite;

// 这个文件里放DataBase的异步接口包装
namespace enigma.DataBase
{
    public partial class DB
    {
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
        public Task<JObject> ExportDataAsync(int from, int to)
        {
            return Task<JObject>.Factory.StartNew(() => ExportData(from, to));
        }

        /// <summary>
        /// 导出所有统计数据
        /// </summary>
        public Task<JObject> DumpStatisticsDataAsync()
        {
            return Task<JObject>.Factory.StartNew(DumpStatisticsData);
        }

        /// <summary>
        /// 导出所有时间标签
        /// </summary>
        public Task<JObject> DumpTimeMarksAsync()
        {
            return Task<JObject>.Factory.StartNew(DumpTimeMarks);
        }

        /// <summary>
        /// 插入一个时间标签，如果id已存在或者不为0则是更新
        /// </summary>
        /// <param name="timeMark"></param>
        public Task AddTimeMarkAsync(TimeMark timeMark)
        {
            return Task.Factory.StartNew(() => AddTimeMark(timeMark));
        }

        /// <summary>
        /// 根据id删除时间标签
        /// </summary>
        /// <param name="id">时间标签的id</param>
        public Task DelTimeMarkAsync(int id)
        {
            return Task.Factory.StartNew(() => DelTimeMark(id));
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

        /// <summary>
        /// 根据时间标签中的信息更新统计数据，不检查时间合法性
        /// </summary>
        /// <param name="timeMark">时间标签</param>
        public Task UpdateTotalAsync(TimeMark timeMark)
        {
            return Task.Factory.StartNew(() => { UpdateTotal(timeMark); });
        }

        /// <summary>
        /// 根据时间标签中的信息更新统计数据，不检查时间合法性
        /// </summary>
        /// <param name="timeMarks">时间标签列表</param>
        public Task UpdateTotalAsync(IEnumerable<TimeMark> timeMarks)
        {
            return Task.Factory.StartNew(() => { UpdateTotal(timeMarks); });
        }

        /// <summary>
        /// 自动更新所有时间标签下的数据
        /// </summary>
        public Task UpdateAllAsync()
        {
            return Task.Factory.StartNew(UpdateAll);
        }
    }
}