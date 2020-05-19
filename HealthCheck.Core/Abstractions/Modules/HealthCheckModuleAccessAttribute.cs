using System;

namespace HealthCheck.Core.Abstractions.Modules
{
    /// <summary>
    /// Sets access for a module method.
    /// <para>Only methods with this attribute will be invokable.</para>
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class HealthCheckModuleAccessAttribute : Attribute
    {
        /// <summary>
        /// TModuleAccessOptionsEnum value.
        /// </summary>
        public object RequiresAccessTo { get; }

        /// <summary>
        /// Sets access for a module method.
        /// <para>Only methods with this attribute will be invokable.</para>
        /// </summary>
        /// <param name="requiresAccessTo">TModuleAccessOptionsEnum value.</param>
        public HealthCheckModuleAccessAttribute(object requiresAccessTo)
        {
            RequiresAccessTo = requiresAccessTo;
        }
    }
}
