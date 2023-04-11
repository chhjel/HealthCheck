using QoDL.Toolkit.Core.Abstractions.Modules;
using System.Collections.Generic;

namespace QoDL.Toolkit.Core.Modules.Tests
{
    internal class TKTestsModuleConfig : IToolkitModuleConfig
    {
        public string Name { get; } = "Tests";
        public string ComponentName => "TestSuitesPageComponent";
        public string DefaultRootRouteSegment => "tests";
        public string InitialRoute => "/{0}";
        public string RoutePath => "/{0}/:group?/:set?/:test?";
        public List<ToolkitLinkTagModel> LinkTags => new();
        public List<ToolkitScriptTagModel> ScriptTags => new();
        public string RawHtml { get; }
    }
}
