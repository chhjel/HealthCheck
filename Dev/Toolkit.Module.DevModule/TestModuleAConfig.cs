using QoDL.Toolkit.Core.Abstractions.Modules;
using System.Collections.Generic;

namespace QoDL.Toolkit.Module.DevModule
{
    public class TestModuleAConfig : IToolkitModuleConfig
    {
        public string Name { get; } = "Custom test";
        public string ComponentName => null;
        public string DefaultRootRouteSegment => "devA";
        public string InitialRoute => "/{0}";
        public string RoutePath => "/{0}/:sub1?/:sub2?";
        public string RawHtml { get; } = "<b>Success?</b><br /><a href=\"/etc\">Test</a>";

        public List<ToolkitLinkTagModel> LinkTags { get; } = new List<ToolkitLinkTagModel>()
        {
            ToolkitLinkTagModel.CreateStylesheet("https://use.fontawesome.com/releases/v5.7.2/css/all.css")
        };

        public List<ToolkitScriptTagModel> ScriptTags { get; } = new List<ToolkitScriptTagModel>()
        {
            //ToolkitScriptTagModel.CreateSrc("https://localhost/test.js")
        };
    }
}
