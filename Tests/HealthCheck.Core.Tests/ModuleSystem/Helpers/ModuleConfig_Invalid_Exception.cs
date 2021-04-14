using HealthCheck.Core.Abstractions.Modules;
using System.Collections.Generic;

namespace HealthCheck.Core.Tests.ModuleSystem.Helpers
{
    public class ModuleConfig_Invalid_Exception : IHealthCheckModuleConfig
    {
        public string Name => "".Substring(55, 10);
        public string ComponentName => null;
        public string DefaultRootRouteSegment => null;
        public string InitialRoute => null;
        public string RoutePath => null;
        public List<HealthCheckLinkTagModel> LinkTags => null;
        public List<HealthCheckScriptTagModel> ScriptTags => null;
        public string RawHtml { get; }
    }
}
