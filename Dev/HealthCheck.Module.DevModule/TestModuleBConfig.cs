using HealthCheck.Core.Abstractions.Modules;
using System.Collections.Generic;

namespace HealthCheck.Module.DevModule
{
    public class TestModuleBConfig : IHealthCheckModuleConfig
    {
        public string Name { get; } = "Custom test";
        public string ComponentName => null;
        public string DefaultRootRouteSegment => "devB";
        public string InitialRoute => "/{0}";
        public string RoutePath => "/{0}/:sub1?/:sub2?";
        public string RawHtml { get; }

        public List<HealthCheckLinkTagModel> LinkTags { get; } = new List<HealthCheckLinkTagModel>()
        {
            //HealthCheckLinkTagModel.CreateStylesheet("https://localhost/teststyle2.css")
        };

        public List<HealthCheckScriptTagModel> ScriptTags { get; } = new List<HealthCheckScriptTagModel>()
        {
            //HealthCheckScriptTagModel.CreateSrc("https://localhost/test2.js")
        };
    }
}
