using HealthCheck.Core.Abstractions.Modules;
using System.Collections.Generic;

namespace HealthCheck.Modules.DevModule
{
    public class TestModuleConfig2 : IHealthCheckModuleConfig
    {
        public string Name { get; } = "Test Module 2";
        public string ComponentName => "test-module-2";

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
