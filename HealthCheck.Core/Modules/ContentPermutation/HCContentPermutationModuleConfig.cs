using HealthCheck.Core.Abstractions.Modules;
using System.Collections.Generic;

namespace HealthCheck.Core.Modules.ContentPermutation
{
    internal class HCContentPermutationModuleConfig : IHealthCheckModuleConfig
    {
        public string Name { get; } = "Content Permutations";
        public string ComponentName => "ContentPermutationPageComponent";
        public string DefaultRootRouteSegment => "contentPermutation";
        public string InitialRoute => "/{0}";
        public string RoutePath => "/{0}/:typeId?";
        public List<HealthCheckLinkTagModel> LinkTags => new();
        public List<HealthCheckScriptTagModel> ScriptTags => new();
        public string RawHtml { get; }
    }
}
