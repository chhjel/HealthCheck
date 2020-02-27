using HealthCheck.Core.Modules.Settings;
using System.Collections.Generic;

namespace HealthCheck.Core.Abstractions
{
    /// <summary>
    /// Provides load/save capabilities for settings.
    /// </summary>
    public interface IHealthCheckSettingsService
    {
        /// <summary>
        /// Load all the available settings.
        /// </summary>
        List<HealthCheckSetting> GetSettingItems();

        /// <summary>
        /// Get the value of the setting with the given id.
        /// </summary>
        T GetValue<T>(string settingId);

        /// <summary>
        /// Save the given settings.
        /// </summary>
        void SaveSettings(IEnumerable<HealthCheckSetting> settings);
    }
}
