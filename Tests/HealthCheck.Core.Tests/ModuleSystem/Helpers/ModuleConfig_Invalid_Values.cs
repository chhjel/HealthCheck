using HealthCheck.Core.Abstractions.Modules;
using System.Collections.Generic;

namespace HealthCheck.Core.Tests.ModuleSystem.Helpers
{
    public class ModuleConfig_Invalid_Values : IHealthCheckModuleConfig
    {
        public string Name => null;
        public string ComponentName => null;
        public string DefaultRootRouteSegment => null;
        public string InitialRoute => null;
        public string RoutePath => null;
        public List<HealthCheckLinkTagModel> LinkTags => null;
        public List<HealthCheckScriptTagModel> ScriptTags => null;
        public string RawHtml { get; }
    }
}
