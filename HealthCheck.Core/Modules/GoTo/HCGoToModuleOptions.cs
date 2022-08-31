using HealthCheck.Core.Modules.GoTo.Abstractions;

namespace HealthCheck.Core.Modules.GoTo
{
    /// <summary>
    /// Options for <see cref="HCGoToModule"/>.
    /// </summary>
    public class HCGoToModuleOptions
    {
        /// <summary>
        /// Service that handles the resolve management.
        /// </summary>
        public IHCGoToService Service { get; set; }
    }
}
