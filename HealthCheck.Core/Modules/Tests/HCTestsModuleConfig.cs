﻿using HealthCheck.Core.Abstractions.Modules;
using System.Collections.Generic;

namespace HealthCheck.Core.Modules.Tests
{
    internal class HCTestsModuleConfig : IHealthCheckModuleConfig
    {
        public string Name { get; } = "Tests";
        public string ComponentName => "TestSuitesPageComponent";
        public string DefaultRootRouteSegment => "tests";
        public string InitialRoute => "/{0}";
        public string RoutePath => "/{0}/:group?/:set?/:test?";
        public List<HealthCheckLinkTagModel> LinkTags => new();
        public List<HealthCheckScriptTagModel> ScriptTags => new();
        public string RawHtml { get; }
    }
}
