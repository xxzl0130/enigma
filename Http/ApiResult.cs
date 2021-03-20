using System;

namespace enigma.Http
{
    /// <summary>
    /// API接口常规返回结果
    /// </summary>
    public class ApiResult
    {
        /// <summary>
        /// API请求是否成功
        /// </summary>
        public bool Ok { get; set; }

        /// <summary>
        /// 信息
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// 数据
        /// </summary>
        public object Data { get; set; }

        /// <summary>
        /// 成功的默认值
        /// </summary>
        public static ApiResult Success = new ApiResult() {Ok = true};

        /// <summary>
        /// 失败的默认值
        /// </summary>
        public static ApiResult Fail = new ApiResult() {Ok = false};

        public ApiResult()
        {
            Ok = true;
            Message = string.Empty;
            Data = null;
        }

        public ApiResult(Exception e)
        {
            Ok = false;
            Message = e.ToString();
            Data = null;
        }
    }
}