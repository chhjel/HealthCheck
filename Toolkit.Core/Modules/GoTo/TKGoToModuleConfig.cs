using QoDL.Toolkit.Core.Abstractions.Modules;
using System.Collections.Generic;

namespace QoDL.Toolkit.Core.Modules.GoTo;

internal class TKGoToModuleConfig : IToolkitModuleConfig
{
    public string Name { get; } = "GoTo";
    public string ComponentName => "GoToPageComponent";
    public string DefaultRootRouteSegment => "goto";
    public string InitialRoute => "/{0}";
    public string RoutePath => "/{0}";
    public List<ToolkitLinkTagModel> LinkTags => new();
    public List<ToolkitScriptTagModel> ScriptTags => new();
    public string RawHtml { get; }
}
