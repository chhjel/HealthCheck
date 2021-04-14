using HealthCheck.Core.Abstractions.Modules;
using System.Collections.Generic;

namespace HealthCheck.Core.Modules.Dataflow
{
    internal class HCDataflowModuleConfig : IHealthCheckModuleConfig
    {
        public string Name { get; } = "DataFlow";
        public string ComponentName => "DataflowPageComponent";
        public string DefaultRootRouteSegment => "dataflow";
        public string InitialRoute => "/{0}";
        public string RoutePath => "/{0}/:group?/:streamName?";
        public List<HealthCheckLinkTagModel> LinkTags => new List<HealthCheckLinkTagModel>();
        public List<HealthCheckScriptTagModel> ScriptTags => new List<HealthCheckScriptTagModel>();
        public string RawHtml { get; }
    }
}
