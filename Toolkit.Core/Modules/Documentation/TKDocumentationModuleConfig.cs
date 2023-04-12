using QoDL.Toolkit.Core.Abstractions.Modules;
using System.Collections.Generic;

namespace QoDL.Toolkit.Core.Modules.Documentation;

internal class TKDocumentationModuleConfig : IToolkitModuleConfig
{
    public string Name { get; } = "Documentation";
    public string ComponentName => "DocumentationPageComponent";
    public string DefaultRootRouteSegment => "documentation";
    public string InitialRoute => "/{0}";
    public string RoutePath => "/{0}/:title?";
    public List<ToolkitLinkTagModel> LinkTags => new();
    public List<ToolkitScriptTagModel> ScriptTags => new();
    public string RawHtml { get; }
}
