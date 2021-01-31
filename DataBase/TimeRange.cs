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

}
