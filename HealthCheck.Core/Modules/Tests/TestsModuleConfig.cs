using HealthCheck.Core.Abstractions.Modules;
using System.Collections.Generic;

namespace HealthCheck.Core.Modules.Tests
{
    internal class TestsModuleConfig : IHealthCheckModuleConfig
    {
        public string Name { get; } = "Tests";
        public string ComponentName => "TestSuitesPageComponent";
        public string DefaultRootRouteSegment => "tests";
        public string InitialRoute => "/{0}";
        public string RoutePath => "/{0}/:group?/:set?/:test?";

        public List<HealthCheckLinkTagModel> LinkTags { get; } = new List<HealthCheckLinkTagModel>()
        {
            HealthCheckLinkTagModel.CreateStylesheet("https://use.fontawesome.com/releases/v5.7.2/css/all.css")
        };

        public List<HealthCheckScriptTagModel> ScriptTags { get; } = new List<HealthCheckScriptTagModel>()
        {
            //HealthCheckScriptTagModel.CreateSrc("https://localhost/test.js")
        };
    }
}
