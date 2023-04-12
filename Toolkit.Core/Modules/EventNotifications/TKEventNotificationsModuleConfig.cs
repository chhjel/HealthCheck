using QoDL.Toolkit.Core.Abstractions.Modules;
using System.Collections.Generic;

namespace QoDL.Toolkit.Core.Modules.EventNotifications;

internal class TKEventNotificationsModuleConfig : IToolkitModuleConfig
{
    public string Name { get; } = "Events";
    public string ComponentName => "EventNotificationsPageComponent";
    public string DefaultRootRouteSegment => "events";
    public string InitialRoute => "/{0}";
    public string RoutePath => "/{0}/:id?";
    public List<ToolkitLinkTagModel> LinkTags => new();
    public List<ToolkitScriptTagModel> ScriptTags => new();
    public string RawHtml { get; }
}
