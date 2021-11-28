using System.Collections.Generic;

namespace HealthCheck.Core.Modules.DataRepeater.Models
{
    /// <summary></summary>
    public class HCDataItemChangeBase
    {
        /// <summary>
        /// Optionally set if item can be retried or not.
        /// </summary>
        public bool? AllowRetry { get; set; }

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
    }
}
