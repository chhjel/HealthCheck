using QoDL.Toolkit.Core.Abstractions.Modules;
using System.Collections.Generic;

namespace QoDL.Toolkit.Core.Modules.Comparison;

internal class TKComparisonModuleConfig : IToolkitModuleConfig
{
    public string Name { get; } = "Comparison";
    public string ComponentName => "ComparisonPageComponent";
    public string DefaultRootRouteSegment => "comparison";
    public string InitialRoute => "/{0}";
    public string RoutePath => "/{0}/:typeId?";
    public List<ToolkitLinkTagModel> LinkTags => new();
    public List<ToolkitScriptTagModel> ScriptTags => new();
    public string RawHtml { get; }
}
