using HealthCheck.Core.Abstractions.Modules;
using System.Collections.Generic;

namespace HealthCheck.Core.Modules.AccessManager
{
    internal class HCAccessManagerModuleConfig : IHealthCheckModuleConfig
    {
        public string Name { get; } = "Access";
        public string ComponentName => "AccessManagerPageComponent";
        public string DefaultRootRouteSegment => "access";
        public string InitialRoute => "/{0}";
        public string RoutePath => "/{0}";
        public List<HealthCheckLinkTagModel> LinkTags => null;
        public List<HealthCheckScriptTagModel> ScriptTags => null;
    }
}
