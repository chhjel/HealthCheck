using HealthCheck.Core.Util.Models;
using System.Collections.Generic;

namespace HealthCheck.Core.Modules.Settings.Models
{
    /// <summary>
    /// Model used when retrieving settings.
    /// </summary>
    public class GetSettingsViewModel
    {
        /// <summary>
        /// Setting definitions.
        /// </summary>
        public List<HCBackendInputConfig> Definitions { get; set; } = new List<HCBackendInputConfig>();

        /// <summary>
        /// Setting values.
        /// </summary>
        public Dictionary<string, string> Values { get; set; } = new Dictionary<string, string>();
    }
}
