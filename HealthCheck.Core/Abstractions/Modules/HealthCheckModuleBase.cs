using System;
using System.Collections.Generic;
using System.Linq;
#pragma warning disable S2326

namespace HealthCheck.Core.Abstractions.Modules
{
    /// <summary>
    /// A custom module visible in the health check.
    /// <para>TModuleAccessOptionsEnum must be a flags enum.</para>
    /// </summary>
    public abstract class HealthCheckModuleBase<TModuleAccessOptionsEnum> : IHealthCheckModule
        where TModuleAccessOptionsEnum : Enum
    {
        /// <inheritdoc />
        public virtual List<string> AllCategories => null;

        /// <summary>
        /// Optional object that will be serialized and used as the options model in frontend for the module pages.
        /// </summary>
        public abstract object GetFrontendOptionsObject(HealthCheckModuleContext context);

        /// <summary>
        /// Optional values to be rendered on the page.
        /// </summary>
        public abstract IHealthCheckModuleConfig GetModuleConfig(HealthCheckModuleContext context);

        /// <summary>
        /// Optionally validate options etc. Return any issues to be displayed.
        /// </summary>
        public virtual IEnumerable<string> Validate() => Enumerable.Empty<string>();
    }
}
