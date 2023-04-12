using QoDL.Toolkit.Core.Abstractions.Modules;
using System.Collections.Generic;

namespace QoDL.Toolkit.Core.Modules.AuditLog;

internal class TKAuditLogModuleConfig : IToolkitModuleConfig
{
    public string Name { get; } = "Audit Log";
    public string ComponentName => "AuditLogPageComponent";
    public string DefaultRootRouteSegment => "auditlog";
    public string InitialRoute => "/{0}";
    public string RoutePath => "/{0}";
    public List<ToolkitLinkTagModel> LinkTags => new();
    public List<ToolkitScriptTagModel> ScriptTags => new();
    public string RawHtml { get; }
}
