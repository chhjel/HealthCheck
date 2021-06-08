using System;

namespace HealthCheck.Core.Models
{
    /// <summary></summary>
    public class HCGenericResult
    {
        /// <summary></summary>
        public bool Success { get; set; }

        /// <summary></summary>
        public bool HasError => !Success;

        /// <summary></summary>
        public string Error { get; set; }

        /// <summary></summary>
        public static HCGenericResult CreateSuccess() => new() { Success = true };

        /// <summary></summary>
        public static HCGenericResult CreateError(string error) => new() { Error = error };

        /// <summary></summary>
        public static HCGenericResult CreateError(Exception ex) => new() { Error = ex?.ToString() };
    }

    /// <summary></summary>
    public class HCGenericResult<TData> : HCGenericResult
    {
        /// <summary></summary>
        public TData Data { get; set; }

        ///// <summary></summary>
        //public static HCGenericResult<T> CreateSuccess<T>(T data) => new() { Success = true, Data = data };

        ///// <summary></summary>
        //public static HCGenericResult<T> CreateError<T>(string error) => new() { Error = error };

        ///// <summary></summary>
        //public static HCGenericResult<T> CreateError<T>(Exception ex) => new() { Error = ex?.ToString() };

        /// <summary></summary>
        public static new HCGenericResult<TData> CreateSuccess(TData data) => new() { Success = true, Data = data };

        /// <summary></summary>
        public static new HCGenericResult<TData> CreateError(string error) => new() { Error = error };

        /// <summary></summary>
        public static new HCGenericResult<TData> CreateError(Exception ex) => new() { Error = ex?.ToString() };
    }
}
