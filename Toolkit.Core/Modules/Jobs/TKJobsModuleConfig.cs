using QoDL.Toolkit.Core.Abstractions.Modules;
using System.Collections.Generic;

namespace QoDL.Toolkit.Core.Modules.Jobs;

internal class TKJobsModuleConfig : IToolkitModuleConfig
{
    public string Name { get; } = "Jobs";
    public string ComponentName => "JobsPageComponent";
    public string DefaultRootRouteSegment => "jobs";
    public string InitialRoute => "/{0}";
    public string RoutePath => "/{0}/:jobId?";
    public List<ToolkitLinkTagModel> LinkTags => new();
    public List<ToolkitScriptTagModel> ScriptTags => new();
    public string RawHtml { get; }
}