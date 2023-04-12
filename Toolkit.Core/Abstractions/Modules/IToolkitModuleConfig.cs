using System.Collections.Generic;

namespace QoDL.Toolkit.Core.Abstractions.Modules;

/// <summary>
/// Page-options for the module.
/// </summary>
public interface IToolkitModuleConfig
{
    /// <summary>
    /// Name of the module.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Default segment value to use.
    /// <para>Value will be used in <see cref="InitialRoute"/> and <see cref="RoutePath"/> unless overridden.</para>
    /// </summary>
    string DefaultRootRouteSegment { get; }

    /// <summary>
    /// Initial route with {0} instead of the first segment name.
    /// <para>{0} will be replaced with <see cref="DefaultRootRouteSegment"/> or overridden value.</para>
    /// </summary>
    string InitialRoute { get; }

    /// <summary>
    /// Full VueRouter route path with {0} instead of the first segment name.
    /// <para>{0} will be replaced with <see cref="DefaultRootRouteSegment"/> or overridden value.</para>
    /// </summary>
    string RoutePath { get; }

    /// <summary>
    /// Vue component name for the module page in frontend.
    /// <para>If <see cref="RawHtml"/> is set this property will have no effect.</para>
    /// </summary>
    string ComponentName { get; }

    /// <summary>
    /// Raw html to show instead of <see cref="ComponentName"/>.
    /// </summary>
    string RawHtml { get; }

    /// <summary>
    /// Any custom link-tags to include.
    /// </summary>
    List<ToolkitLinkTagModel> LinkTags { get; }

    /// <summary>
    /// Any custom script-tags to include.
    /// </summary>
    List<ToolkitScriptTagModel> ScriptTags { get; }
}