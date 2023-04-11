using QoDL.Toolkit.Core.Abstractions.Modules;
using System.Collections.Generic;

namespace QoDL.Toolkit.Core.Modules.ContentPermutation
{
    internal class TKContentPermutationModuleConfig : IToolkitModuleConfig
    {
        public string Name { get; } = "Content Permutations";
        public string ComponentName => "ContentPermutationPageComponent";
        public string DefaultRootRouteSegment => "contentPermutation";
        public string InitialRoute => "/{0}";
        public string RoutePath => "/{0}/:typeId?";
        public List<ToolkitLinkTagModel> LinkTags => new();
        public List<ToolkitScriptTagModel> ScriptTags => new();
        public string RawHtml { get; }
    }
}
