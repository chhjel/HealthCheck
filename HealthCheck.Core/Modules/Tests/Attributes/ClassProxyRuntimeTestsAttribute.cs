using HealthCheck.Core.Modules.Tests.Models;
using System;

namespace HealthCheck.Core.Modules.Tests.Attributes
{
    /// <summary>
    /// Automatically creates tests for each public method on a given class type.
    /// <para>Return an <see cref="ClassProxyRuntimeTestConfig"/> instance from this method with the setup details.</para>
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class ClassProxyRuntimeTestsAttribute : Attribute
    {
        /// <summary>
        /// Set roles that are allowed access to the tests.
        /// <para>Must be an enum flags value.</para>
        /// </summary>
        public object RolesWithAccess { get; set; }
    }
}
