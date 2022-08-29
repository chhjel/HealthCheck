﻿using HealthCheck.Core.Abstractions.Modules;
using System.Collections.Generic;

namespace HealthCheck.Core.Modules.Comparison
{
    internal class HCComparisonModuleConfig : IHealthCheckModuleConfig
    {
        public string Name { get; } = "Comparison";
        public string ComponentName => "ComparisonPageComponent";
        public string DefaultRootRouteSegment => "comparison";
        public string InitialRoute => "/{0}";
        public string RoutePath => "/{0}/:typeId?";
        public List<HealthCheckLinkTagModel> LinkTags => new();
        public List<HealthCheckScriptTagModel> ScriptTags => new();
        public string RawHtml { get; }
    }
}