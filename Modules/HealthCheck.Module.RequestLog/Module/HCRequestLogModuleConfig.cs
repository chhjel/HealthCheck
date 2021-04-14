﻿using HealthCheck.Core.Abstractions.Modules;
using System.Collections.Generic;

namespace HealthCheck.Core.Modules.Settings
{
    internal class HCRequestLogModuleConfig : IHealthCheckModuleConfig
    {
        public string Name { get; } = "Requests";
        public string ComponentName => "RequestLogPageComponent";
        public string DefaultRootRouteSegment => "requestlog";
        public string InitialRoute => "/{0}";
        public string RoutePath => "/{0}";
        public List<HealthCheckLinkTagModel> LinkTags => new List<HealthCheckLinkTagModel>();
        public List<HealthCheckScriptTagModel> ScriptTags => new List<HealthCheckScriptTagModel>();
        public string RawHtml { get; }
    }
}
