using System;
using System.Collections.Generic;

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

        /// <summary>
        /// Optionally configure if further retrying should be allowed.
        /// </summary>
        public bool? AllowRetry { get; set; }

        /// <summary>
        /// Optionally delete the item.
        /// </summary>
        public bool Delete { get; set; }

        /// <summary>
        /// Optionally remove all item tags.
        /// <para><see cref="TagsThatShouldExist"/> will still take effect.</para>
        /// </summary>
        public bool RemoveAllTags { get; set; }

        /// <summary>
        /// Tags that will be applied if missing.
        /// </summary>
        public List<string> TagsThatShouldExist { get; set; } = new();

        /// <summary>
        /// Tags that will be removed if present.
        /// </summary>
        public List<string> TagsThatShouldNotExist { get; set; } = new();

        /// <summary></summary>
        public static HCDataRepeaterActionResult CreateSuccess() => new() { Success = true };

        /// <summary></summary>
        public static HCDataRepeaterActionResult CreateError(string error) => new() { Error = error };

        /// <summary></summary>
        public static HCDataRepeaterActionResult CreateError(Exception ex) => new() { Error = ex?.ToString() };
    }
}
