using HealthCheck.Core.Abstractions.Modules;
using System.Collections.Generic;

namespace HealthCheck.Module.DevModule
{
    public class TestModuleAConfig : IHealthCheckModuleConfig
    {
        public string Name { get; } = "Custom test";
        public string ComponentName => null;
        public string DefaultRootRouteSegment => "devA";
        public string InitialRoute => "/{0}";
        public string RoutePath => "/{0}/:sub1?/:sub2?";
        public string RawHtml { get; } = "<b>Success?</b><br /><a href=\"/etc\">Test</a>";

        public List<HealthCheckLinkTagModel> LinkTags { get; } = new List<HealthCheckLinkTagModel>()
        {
#pragma warning disable S1075 // URIs should not be hardcoded
            HealthCheckLinkTagModel.CreateStylesheet("https://use.fontawesome.com/releases/v5.7.2/css/all.css")
#pragma warning restore S1075 // URIs should not be hardcoded
        };

        public List<HealthCheckScriptTagModel> ScriptTags { get; } = new List<HealthCheckScriptTagModel>()
        {
            //HealthCheckScriptTagModel.CreateSrc("https://localhost/test.js")
        };
    }
}
