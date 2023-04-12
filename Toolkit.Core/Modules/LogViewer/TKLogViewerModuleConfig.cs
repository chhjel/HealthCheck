using QoDL.Toolkit.Core.Abstractions.Modules;
using System.Collections.Generic;

namespace QoDL.Toolkit.Core.Modules.LogViewer;

internal class TKLogViewerModuleConfig : IToolkitModuleConfig
{
    public string Name { get; } = "Logs";
    public string ComponentName => "LogViewerPageComponent";
    public string DefaultRootRouteSegment => "logs";
    public string InitialRoute => "/{0}";
    public string RoutePath => "/{0}";
    public List<ToolkitLinkTagModel> LinkTags => new();
    public List<ToolkitScriptTagModel> ScriptTags => new();
    public string RawHtml { get; }
}
