using System;

namespace HealthCheck.Core.Models
{
    /// <summary></summary>
    public class HCGenericResult<TData>
    {
        /// <summary></summary>
        public bool Success { get; set; }

        /// <summary></summary>
        public bool HasError => !Success;

        /// <summary></summary>
        public string Error { get; set; }

        /// <summary></summary>
        public TData Data { get; set; }

        /// <summary></summary>
        public static HCGenericResult<TData> CreateSuccess(TData data) => new HCGenericResult<TData> { Success = true, Data = data };

        /// <summary></summary>
        public static HCGenericResult<TData> CreateError(string error) => new HCGenericResult<TData> { Error = error };

        /// <summary></summary>
        public static HCGenericResult<TData> CreateError(Exception ex) => new HCGenericResult<TData> { Error = ex?.ToString() };
    }
}
