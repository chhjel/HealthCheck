using HealthCheck.Core.Abstractions.Modules;
using System.Collections.Generic;

namespace HealthCheck.Modules.DevModule
{
    public class TestModuleAConfig : IHealthCheckModuleConfig
    {
        public string Name { get; } = "Test Module A";
        public string ComponentName => "test-module-a";
        public string DefaultRootRouteSegment => "devA";
        public string InitialRoute => "/{0}";
        public string RoutePath => "/{0}/:sub1?/:sub2?";

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
