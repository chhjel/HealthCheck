using HealthCheck.Core.Abstractions.Modules;
using System.Collections.Generic;

namespace HealthCheck.Module.EndpointControl.Module
{
    internal class HCEndpointControlModuleConfig : IHealthCheckModuleConfig
    {
        public string Name { get; } = "Endpoints";
        public string ComponentName => "EndpointControlPageComponent";
        public string DefaultRootRouteSegment => "endpoints";
        public string InitialRoute => "/{0}";
        public string RoutePath => "/{0}/:id?";
        public List<HealthCheckLinkTagModel> LinkTags => new List<HealthCheckLinkTagModel>();
        public List<HealthCheckScriptTagModel> ScriptTags => new List<HealthCheckScriptTagModel>();
    }
}
