using System;

namespace HealthCheck.Core.Abstractions.Modules
{
    /// <summary>
    /// Methods must be decorated with this attribute to be invokable.
    /// <para>First parameter can optionaly be a <see cref="HealthCheckModuleContext"/></para>
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class HealthCheckModuleMethodAttribute : Attribute
    {
        /// <summary>
        /// Sets access required to invoke the method.
        /// <para>Must be a TModuleAccessOptionsEnum enum flags value.</para>
        /// <para>If null any role that can view the module has access to invoke this method.</para>
        /// </summary>
        public object RequiresAccessTo { get; }

        /// <summary>
        /// Methods must be decorated with this attribute to be invokable.
        /// <para>First parameter can optionaly be a <see cref="HealthCheckModuleContext"/></para>
        /// </summary>
        /// <param name="requiresAccessTo">
        /// Optional access required to invoke the method. Must be a TModuleAccessOptionsEnum enum flags value or null.
        /// <para>If null any role that can view the module has access to invoke this method.</para>
        /// </param>
        public HealthCheckModuleMethodAttribute(object requiresAccessTo = null)
        {
            RequiresAccessTo = requiresAccessTo;
        }
    }
}
