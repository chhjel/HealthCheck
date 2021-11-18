using HealthCheck.Core.Abstractions.Modules;
using System.Collections.Generic;

namespace HealthCheck.Core.Modules.Metrics
{
    internal class HCMetricsModuleConfig : IHealthCheckModuleConfig
    {
        public string Name { get; } = "Metrics";
        public string ComponentName => "MetricsPageComponent";
        public string DefaultRootRouteSegment => "metrics";
        public string InitialRoute => "/{0}";
        public string RoutePath => "/{0}";
        public List<HealthCheckLinkTagModel> LinkTags => new();
        public List<HealthCheckScriptTagModel> ScriptTags => new();
        public string RawHtml { get; }
    }
}
