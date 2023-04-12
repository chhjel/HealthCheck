using QoDL.Toolkit.Core.Abstractions.Modules;
using System.Collections.Generic;

namespace QoDL.Toolkit.Core.Tests.ModuleSystem.Helpers;

public class ModuleConfig_Invalid_Values : IToolkitModuleConfig
{
    public string Name => null;
    public string ComponentName => null;
    public string DefaultRootRouteSegment => null;
    public string InitialRoute => null;
    public string RoutePath => null;
    public List<ToolkitLinkTagModel> LinkTags => null;
    public List<ToolkitScriptTagModel> ScriptTags => null;
    public string RawHtml { get; }
}
