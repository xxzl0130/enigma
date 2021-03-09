using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using SQLite;

namespace enigma
{
    /// <summary>
    /// 和范围的关系类型
    /// </summary>
    public enum RangeType
    {
        /// <summary>
        /// 在范围内
        /// </summary>
        In,
        /// <summary>
        /// 不在范围内
        /// </summary>
        NotIn
    }

    /// <summary>
    /// 时间范围，包含起止时间戳和类型
    /// </summary>
    public class TimeRange
    {
        /// <summary>
        /// 类型
        /// </summary>
        public RangeType Type { get; set; }
        /// <summary>
        /// 起始时间UTC时间戳
        /// </summary>
        public int Start { get; set; }
        /// <summary>
        /// 结束时间UTC时间戳
        /// </summary>
        public int End { get; set; }

        /// <summary>
        /// 转换为SQL语句
        /// </summary>
        /// <param name="colName">列名称</param>
        /// <returns>SQL语句</returns>
        public string ToSQL(string colName)
        {
            var sb = new StringBuilder();
            sb.Append(colName)
                .Append(Type == RangeType.In ? " BETWEEN " : " NOT BETWEEN ")
                .Append(Start)
                .Append(" AND ")
                .Append(End);
            return sb.ToString();
        }

        /// <summary>
        /// 将一系列时间范围转换为SQL语句
        /// </summary>
        /// <param name="list">时间范围</param>
        /// <param name="colName">列名称</param>
        /// <returns>SQL语句</returns>
        public static string TimeRangeList2SQL(IEnumerable<TimeRange> list, string colName)
        {
            var sb = new StringBuilder();
            foreach (var time in list)
            {
                sb.Append(time.ToSQL(colName))
                    .Append(" AND ");
            }
            
            // 排除最后一个and
            return sb.ToString(0, sb.Length - 5);
        }
    }

    /// <summary>
    /// 时间标记
    /// </summary>
    public class TimeMark
    {
        /// <summary>
        /// 时间id
        /// </summary>
        [PrimaryKey, AutoIncrement]
        public int ID { get; set; }
        /// <summary>
        /// 表示时间范围的集合
        /// </summary>
        [Ignore]
        public List<TimeRange> TimeRanges { get; set; }
        /// <summary>
        /// TimeRanges的JSON字符串接口，供保存SQLite
        /// </summary>
        [Column("TimeRanges")]
        public string TimeRangesString
        {
            get => JsonConvert.SerializeObject(TimeRanges);
            set => TimeRanges = JsonConvert.DeserializeObject<List<TimeRange>>(value);
        }
        /// <summary>
        /// 更新截止时间，过时不再更新，负数为永不过期
        /// </summary>
        public int UpdateLimitTime { get; set; }
        /// <summary>
        /// 更新间隔，负数为永不更新
        /// </summary>
        public int UpdateInterval { get; set; }
        /// <summary>
        /// 最后更新时间
        /// </summary>
        public int LastUpdateTime { get; set; }
        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }

        public TimeMark()
        {
            TimeRanges = new List<TimeRange>();
            UpdateInterval = -1;
            LastUpdateTime = 0;
            UpdateLimitTime = -1;
            ID = 0;
            Name = string.Empty;
        }

        public TimeMark(TimeRange timeRange, int id = 0)
        {
            TimeRanges = new List<TimeRange>(){timeRange};
            ID = id;
            UpdateInterval = -1;
            LastUpdateTime = 0;
            UpdateLimitTime = -1;
            Name = string.Empty;
        }
    }
}
