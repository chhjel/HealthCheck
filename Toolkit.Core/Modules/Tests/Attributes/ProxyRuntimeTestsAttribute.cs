using QoDL.Toolkit.Core.Modules.Tests.Models;
using System;

namespace QoDL.Toolkit.Core.Modules.Tests.Attributes;

/// <summary>
/// Automatically creates tests for each public method on a given class type.
/// <para>Return an <see cref="ProxyRuntimeTestConfig"/> instance from this method with the setup details.</para>
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public class ProxyRuntimeTestsAttribute : Attribute
{
    /// <summary>
    /// Set roles that are allowed access to the tests.
    /// <para>Must be an enum flags value.</para>
    /// </summary>
    public object RolesWithAccess { get; set; }
}
