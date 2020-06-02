using HealthCheck.Core.Modules.AccessManager.Abstractions;
using System;

namespace HealthCheck.Core.Modules.AccessManager
{
    /// <summary>
    /// Options for <see cref="HCAccessManagerModule"/>.
    /// </summary>
    public class HCAccessManagerModuleOptions
    {
        /// <summary>
        /// Provides storage for generated access tokens.
        /// </summary>
        public IAccessManagerTokenStorage TokenStorage { get; set; }
    }
}
