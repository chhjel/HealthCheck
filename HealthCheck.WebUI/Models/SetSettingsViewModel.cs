using HealthCheck.Core.Modules.Settings;
using System.Collections.Generic;

namespace HealthCheck.WebUI.Models
{
    /// <summary>
    /// Model used when updating settings.
    /// </summary>
    public class SetSettingsViewModel
    {
        /// <summary>
        /// Settings that will be set.
        /// </summary>
        public IEnumerable<HealthCheckSetting> Settings { get; set; }
    }
}
