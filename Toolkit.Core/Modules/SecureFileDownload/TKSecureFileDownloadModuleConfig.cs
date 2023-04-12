using QoDL.Toolkit.Core.Abstractions.Modules;
using System.Collections.Generic;

namespace QoDL.Toolkit.Core.Modules.SecureFileDownload;

internal class TKSecureFileDownloadModuleConfig : IToolkitModuleConfig
{
    public string Name { get; } = "Downloads";
    public string ComponentName => "SecureFileDownloadPageComponent";
    public string DefaultRootRouteSegment => "downloads";
    public string InitialRoute => "/{0}";
    public string RoutePath => "/{0}/:id?";
    public List<ToolkitLinkTagModel> LinkTags => new();
    public List<ToolkitScriptTagModel> ScriptTags => new();
    public string RawHtml { get; }
}
