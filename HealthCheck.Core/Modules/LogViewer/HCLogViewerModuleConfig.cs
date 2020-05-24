﻿using HealthCheck.Core.Abstractions.Modules;
using System.Collections.Generic;

namespace HealthCheck.Core.Modules.LogViewer
{
    internal class HCLogViewerModuleConfig : IHealthCheckModuleConfig
    {
        public string Name { get; } = "Logs";
        public string ComponentName => "LogViewerPageComponent";
        public string DefaultRootRouteSegment => "logs";
        public string InitialRoute => "/{0}";
        public string RoutePath => "/{0}";
        public List<HealthCheckLinkTagModel> LinkTags => null;
        public List<HealthCheckScriptTagModel> ScriptTags => null;
    }
}