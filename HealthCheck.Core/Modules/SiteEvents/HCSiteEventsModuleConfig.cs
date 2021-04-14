﻿using HealthCheck.Core.Abstractions.Modules;
using System.Collections.Generic;

namespace HealthCheck.Core.Modules.SiteEvents
{
    internal class HCSiteEventsModuleConfig : IHealthCheckModuleConfig
    {
        public string Name { get; } = "Overview";
        public string ComponentName => "OverviewPageComponent";
        public string DefaultRootRouteSegment => "overview";
        public string InitialRoute => "/{0}";
        public string RoutePath => "/{0}";
        public List<HealthCheckLinkTagModel> LinkTags => new List<HealthCheckLinkTagModel>();
        public List<HealthCheckScriptTagModel> ScriptTags => new List<HealthCheckScriptTagModel>();
        public string RawHtml { get; }
    }
}
