using HealthCheck.Core.Modules.Settings.Abstractions;
using System;

namespace HealthCheck.Core.Modules.Settings
{
    /// <summary>
    /// Options for <see cref="HCSettingsModule"/>.
    /// </summary>
    public class HCSettingsModuleOptions
    {
        /// <summary>
        /// Model type for settings.
        /// </summary>
        public Type ModelType { get; set; }

        /// <summary>
        /// Gets and sets settings.
        /// </summary>
        public IHCSettingsService Service { get; set; }
    }
}
