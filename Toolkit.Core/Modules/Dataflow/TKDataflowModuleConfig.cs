using QoDL.Toolkit.Core.Abstractions.Modules;
using System.Collections.Generic;

namespace QoDL.Toolkit.Core.Modules.Dataflow;

internal class TKDataflowModuleConfig : IToolkitModuleConfig
{
    public string Name { get; } = "DataFlow";
    public string ComponentName => "DataflowPageComponent";
    public string DefaultRootRouteSegment => "dataflow";
    public string InitialRoute => "/{0}";
    public string RoutePath => "/{0}/:group?/:streamName?";
    public List<ToolkitLinkTagModel> LinkTags => new();
    public List<ToolkitScriptTagModel> ScriptTags => new();
    public string RawHtml { get; }
}
