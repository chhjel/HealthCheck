using System;

namespace HealthCheck.Core.Abstractions.Modules
{
    /// <summary>
    /// A custom module visible in the health check.
    /// <para>TModuleAccessOptionsEnum must be a flags enum.</para>
    /// </summary>
    public abstract class HealthCheckModuleBase<TModuleAccessOptionsEnum> : IHealthCheckModule
        where TModuleAccessOptionsEnum : Enum
    {
        /// <summary>
        /// Optional object that will be serialized and used as the options model in frontend for the module pages.
        /// </summary>
        public abstract object GetFrontendOptionsObject(TModuleAccessOptionsEnum access);

        /// <summary>
        /// Optional values to be rendered on the page.
        /// </summary>
        public abstract IHealthCheckModuleConfig GetModuleConfig(TModuleAccessOptionsEnum access);
    }
}
