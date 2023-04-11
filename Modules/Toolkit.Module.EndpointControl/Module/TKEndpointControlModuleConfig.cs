using QoDL.Toolkit.Core.Abstractions.Modules;
using System.Collections.Generic;

namespace QoDL.Toolkit.Module.EndpointControl.Module;

internal class TKEndpointControlModuleConfig : IToolkitModuleConfig
{
    public string Name { get; } = "Endpoints";
    public string ComponentName => "EndpointControlPageComponent";
    public string DefaultRootRouteSegment => "endpoints";
    public string InitialRoute => "/{0}";
    public string RoutePath => "/{0}/:id?";
    public List<ToolkitLinkTagModel> LinkTags => new();
    public List<ToolkitScriptTagModel> ScriptTags => new();
    public string RawHtml { get; }
}
