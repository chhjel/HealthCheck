using QoDL.Toolkit.Core.Abstractions.Modules;
using System.Collections.Generic;

namespace QoDL.Toolkit.Module.DevModule
{
    public class TestModuleBConfig : IToolkitModuleConfig
    {
        public string Name { get; } = "Custom test";
        public string ComponentName => null;
        public string DefaultRootRouteSegment => "devB";
        public string InitialRoute => "/{0}";
        public string RoutePath => "/{0}/:sub1?/:sub2?";
        public string RawHtml { get; }

        public List<ToolkitLinkTagModel> LinkTags { get; } = new List<ToolkitLinkTagModel>()
        {
            //ToolkitLinkTagModel.CreateStylesheet("https://localhost/teststyle2.css")
        };

        public List<ToolkitScriptTagModel> ScriptTags { get; } = new List<ToolkitScriptTagModel>()
        {
            //ToolkitScriptTagModel.CreateSrc("https://localhost/test2.js")
        };
    }
}
