using HealthCheck.Core.Abstractions.Modules;
using System.Collections.Generic;

namespace HealthCheck.Core.Modules.EventNotifications
{
    internal class HCEventNotificationsModuleConfig : IHealthCheckModuleConfig
    {
        public string Name { get; } = "Events";
        public string ComponentName => "EventNotificationsPageComponent";
        public string DefaultRootRouteSegment => "events";
        public string InitialRoute => "/{0}";
        public string RoutePath => "/{0}";
        public List<HealthCheckLinkTagModel> LinkTags => null;
        public List<HealthCheckScriptTagModel> ScriptTags => null;
    }
}
