using System;
using System.Collections.Generic;
using System.Text;

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
        public RangeType Type;
        /// <summary>
        /// 起始时间UTC时间戳
        /// </summary>
        public int Start;
        /// <summary>
        /// 结束时间UTC时间戳
        /// </summary>
        public int End;

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
        /// 表示时间范围的集合
        /// </summary>
        public List<TimeRange> TimeRanges = new List<TimeRange>();
        /// <summary>
        /// 时间id
        /// </summary>
        public int ID = 0;
        /// <summary>
        /// 更新截止时间，过时不再更新，负数为永不过期
        /// </summary>
        public int UpdateLimitTime = 0;
        /// <summary>
        /// 更新间隔，负数为永不更新
        /// </summary>
        public int UpdateInterval = -1;
        /// <summary>
        /// 最后更新时间
        /// </summary>
        public int LastUpdateTime = -1;

        public TimeMark()
        {
        }

        public TimeMark(TimeRange timeRange, int id = 0)
        {
            TimeRanges = new List<TimeRange>(){timeRange};
            ID = id;
        }
    }
}
