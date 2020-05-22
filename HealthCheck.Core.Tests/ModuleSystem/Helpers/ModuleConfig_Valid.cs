using HealthCheck.Core.Abstractions.Modules;
using System.Collections.Generic;

namespace HealthCheck.Core.Tests.ModuleSystem.Helpers
{
    public class ModuleConfig_Valid : IHealthCheckModuleConfig
    {
        public string Name => "Valid Module";
        public string ComponentName => "ValidModuleComponent";
        public string DefaultRootRouteSegment => "validmodule";
        public string InitialRoute => "/{0}";
        public string RoutePath => "/{0}/:id?";
        public List<HealthCheckLinkTagModel> LinkTags => null;
        public List<HealthCheckScriptTagModel> ScriptTags => null;
    }
}
