using System.Collections.Generic;

namespace HealthCheck.Core.Abstractions.Modules
{
    /// <summary>
    /// Inherit from <see cref="HealthCheckModuleBase{TModuleAccessOptionsEnum}"/> instead of this one.
    /// </summary>
    public interface IHealthCheckModule
    {
        /// <summary>
        /// All categories defined for the module if any.
        /// </summary>
        List<string> AllCategories { get; }
    }
}
