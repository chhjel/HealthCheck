using QoDL.Toolkit.Core.Abstractions.Modules;
using System.Collections.Generic;

namespace QoDL.Toolkit.Core.Tests.ModuleSystem.Helpers;

public class ModuleConfig_Valid : IToolkitModuleConfig
{
    public string Name => "Valid Module";
    public string ComponentName => "ValidModuleComponent";
    public string DefaultRootRouteSegment => "validmodule";
    public string InitialRoute => "/{0}";
    public string RoutePath => "/{0}/:id?";
    public List<ToolkitLinkTagModel> LinkTags => null;
    public List<ToolkitScriptTagModel> ScriptTags => null;
    public string RawHtml { get; }
}
