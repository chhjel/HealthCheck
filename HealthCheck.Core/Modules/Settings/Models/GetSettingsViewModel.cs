using System.Collections.Generic;
using System.Linq;

namespace HealthCheck.Core.Modules.Settings.Models
{
    /// <summary>
    /// Model used when retrieving settings.
    /// </summary>
    public class GetSettingsViewModel
    {
        /// <summary>
        /// Setting values.
        /// </summary>
        public IEnumerable<HealthCheckSetting> Settings { get; set; } = Enumerable.Empty<HealthCheckSetting>();
    }
}
