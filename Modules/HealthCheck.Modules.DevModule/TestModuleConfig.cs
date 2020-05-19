using HealthCheck.Core.Abstractions.Modules;
using System.Collections.Generic;

namespace HealthCheck.Modules.DevModule
{
    public class TestModuleConfig : IHealthCheckModuleConfig
    {
        public string Name { get; } = "Test Module";
        public string ComponentName => "test-module-1";

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
