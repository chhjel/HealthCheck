using QoDL.Toolkit.Core.Abstractions.Modules;
using System.Collections.Generic;

namespace QoDL.Toolkit.Module.IPWhitelist.Module;

internal class TKIPWhitelistModuleConfig : IToolkitModuleConfig
{
    public string Name { get; } = "IP Whitelist";
    public string ComponentName => "IPWhitelistPageComponent";
    public string DefaultRootRouteSegment => "ipwhitelist";
    public string InitialRoute => "/{0}/Whitelist";
    public string RoutePath => "/{0}/:tab/:id?";
    public List<ToolkitLinkTagModel> LinkTags => new();
    public List<ToolkitScriptTagModel> ScriptTags => new();
    public string RawHtml { get; }
}
