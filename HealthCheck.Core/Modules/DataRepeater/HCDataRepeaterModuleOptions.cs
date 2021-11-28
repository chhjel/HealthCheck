using HealthCheck.Core.Modules.DataRepeater.Abstractions;

namespace HealthCheck.Core.Modules.DataRepeater
{
    /// <summary>
    /// Options for <see cref="HCDataRepeaterModule"/>.
    /// </summary>
    public class HCDataRepeaterModuleOptions
    {
        /// <summary>
        /// Service that handles the repeating.
        /// </summary>
        public IHCDataRepeaterService Service { get; set; }
    }
}
