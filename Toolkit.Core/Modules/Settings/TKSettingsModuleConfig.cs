using QoDL.Toolkit.Core.Abstractions.Modules;
using System.Collections.Generic;

namespace QoDL.Toolkit.Core.Modules.Settings;

internal class TKSettingsModuleConfig : IToolkitModuleConfig
{
    public string Name { get; } = "Settings";
    public string ComponentName => "SettingsPageComponent";
    public string DefaultRootRouteSegment => "settings";
    public string InitialRoute => "/{0}";
    public string RoutePath => "/{0}";
    public List<ToolkitLinkTagModel> LinkTags => new();
    public List<ToolkitScriptTagModel> ScriptTags => new();
    public string RawHtml { get; }
}
