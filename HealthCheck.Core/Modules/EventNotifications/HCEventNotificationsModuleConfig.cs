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
        public string RoutePath => "/{0}/:id?";
        public List<HealthCheckLinkTagModel> LinkTags => new List<HealthCheckLinkTagModel>();
        public List<HealthCheckScriptTagModel> ScriptTags => new List<HealthCheckScriptTagModel>();
        public string RawHtml { get; }
    }
}
