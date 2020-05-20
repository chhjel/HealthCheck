using HealthCheck.Core.Abstractions;

namespace HealthCheck.Core.Modules.Settings
{
    /// <summary>
    /// Options for <see cref="HCSettingsModule"/>.
    /// </summary>
    public class HCSettingsModuleOptions
    {
        /// <summary>
        /// Gets and sets settings.
        /// </summary>
        public IHealthCheckSettingsService SettingsService { get; set; }
    }
}
