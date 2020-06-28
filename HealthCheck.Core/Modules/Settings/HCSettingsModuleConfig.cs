using HealthCheck.Core.Abstractions.Modules;
using System.Collections.Generic;

namespace HealthCheck.Core.Modules.Settings
{
    internal class HCSettingsModuleConfig : IHealthCheckModuleConfig
    {
        public string Name { get; } = "Settings";
        public string ComponentName => "SettingsPageComponent";
        public string DefaultRootRouteSegment => "settings";
        public string InitialRoute => "/{0}";
        public string RoutePath => "/{0}";
        public List<HealthCheckLinkTagModel> LinkTags => new List<HealthCheckLinkTagModel>();
        public List<HealthCheckScriptTagModel> ScriptTags => new List<HealthCheckScriptTagModel>();
    }
}
