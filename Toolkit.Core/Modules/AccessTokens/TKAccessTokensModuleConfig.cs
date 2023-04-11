using QoDL.Toolkit.Core.Abstractions.Modules;
using System.Collections.Generic;

namespace QoDL.Toolkit.Core.Modules.AccessTokens;

internal class TKAccessTokensModuleConfig : IToolkitModuleConfig
{
    public string Name { get; } = "Tokens";
    public string ComponentName => "AccessTokensPageComponent";
    public string DefaultRootRouteSegment => "tokens";
    public string InitialRoute => "/{0}";
    public string RoutePath => "/{0}";
    public List<ToolkitLinkTagModel> LinkTags => new();
    public List<ToolkitScriptTagModel> ScriptTags => new();
    public string RawHtml { get; }
}
