using QoDL.Toolkit.Core.Modules.Settings.Abstractions;
using System;

namespace QoDL.Toolkit.Core.Modules.Settings
{
    /// <summary>
    /// Options for <see cref="TKSettingsModule"/>.
    /// </summary>
    public class TKSettingsModuleOptions
    {
        /// <summary>
        /// Model type for settings.
        /// </summary>
        public Type ModelType { get; set; }

        /// <summary>
        /// Gets and sets settings.
        /// </summary>
        public ITKSettingsService Service { get; set; }
    }
}
