using System;

namespace QoDL.Toolkit.Core.Abstractions.Modules;

/// <summary>
/// Methods must be decorated with this attribute to be invokable through controller actions with the same name.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class ToolkitModuleActionAttribute : Attribute
{
    /// <summary>
    /// Sets access required to invoke the method.
    /// <para>Must be a TModuleAccessOptionsEnum enum flags value.</para>
    /// <para>If null any role that can view the module has access to invoke this method.</para>
    /// </summary>
    public object RequiresAccessTo { get; }

    /// <summary>
    /// Methods must be decorated with this attribute to be invokable through controller actions with the same name.
    /// </summary>
    /// <param name="requiresAccessTo">
    /// Optional access required to invoke the method. Must be a TModuleAccessOptionsEnum enum flags value or null.
    /// <para>If null any role that can view the module has access to invoke this method.</para>
    /// </param>
    public ToolkitModuleActionAttribute(object requiresAccessTo = null)
    {
        RequiresAccessTo = requiresAccessTo;
    }
}
