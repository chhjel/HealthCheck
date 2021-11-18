using System;

namespace HealthCheck.Core.Modules.DataRepeater.Models
{
    /// <summary></summary>
    public class HCDataRepeaterActionResult
    {
        /// <summary></summary>
        public bool Success { get; set; }

        /// <summary></summary>
        public bool HasError => !Success;

        /// <summary></summary>
        public string Error { get; set; }

        /// <summary></summary>
        public static HCDataRepeaterActionResult CreateSuccess() => new() { Success = true };

        /// <summary></summary>
        public static HCDataRepeaterActionResult CreateError(string error) => new() { Error = error };

        /// <summary></summary>
        public static HCDataRepeaterActionResult CreateError(Exception ex) => new() { Error = ex?.ToString() };
    }
}
