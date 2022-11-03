using HealthCheck.Core.Abstractions.Modules;
using System.Collections.Generic;

namespace HealthCheck.Core.Modules.Jobs
{
    internal class HCJobsModuleConfig : IHealthCheckModuleConfig
    {
        public string Name { get; } = "Jobs";
        public string ComponentName => "JobsPageComponent";
        public string DefaultRootRouteSegment => "jobs";
        public string InitialRoute => "/{0}";
        public string RoutePath => "/{0}";
        public List<HealthCheckLinkTagModel> LinkTags => new();
        public List<HealthCheckScriptTagModel> ScriptTags => new();
        public string RawHtml { get; }
    }
}