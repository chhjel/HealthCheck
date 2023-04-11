using QoDL.Toolkit.Core.Abstractions.Modules;
using QoDL.Toolkit.Core.Tests.ModuleSystem.Helpers;
using QoDL.Toolkit.Core.Util.Modules;
using Xunit;
using Xunit.Abstractions;

namespace QoDL.Toolkit.Core.Tests.ModuleSystem;

public class LoadModuleTests
{
    public ITestOutputHelper Output { get; }

    public LoadModuleTests(ITestOutputHelper output)
    {
        Output = output;
    }

    [Fact]
    public void Load_ValidModule_NoAccessOptions_DoesNotFail()
    {
        var loader = new ToolkitModuleLoader();
        var module = loader.Load(new Module_Valid(), CreateContext(ModuleAccessOptions_Valid.None));
        Assert.True(module.LoadedSuccessfully);
    }

    [Fact]
    public void Load_ValidModule_SomeAccessOptions_DoesNotFail()
    {
        var loader = new ToolkitModuleLoader();
        var module = loader.Load(new Module_Valid(), CreateContext(ModuleAccessOptions_Valid.DeleteEverything | ModuleAccessOptions_Valid.Read));
        Assert.True(module.LoadedSuccessfully);
    }

    [Fact]
    public void Load_ValidModule_CustomName_NameIsSet()
    {
        var loader = new ToolkitModuleLoader();
        var module = loader.Load(new Module_Valid(), CreateContext(ModuleAccessOptions_Valid.Write), "Custom Name");
        Assert.True(module.LoadedSuccessfully);
        Assert.Equal("Custom Name", module.Name);
    }

    [Fact]
    public void Load_InvalidModule_NoConfig_ReportsErrors()
    {
        var loader = new ToolkitModuleLoader();
        var module = loader.Load(new Module_Invalid_NoConfig(), CreateContext(ModuleAccessOptions_Valid.None));
        Assert.True(!module.LoadedSuccessfully);
    }

    [Fact]
    public void Load_InvalidModule_NotFlagsEnum_ReportsErrors()
    {
        var loader = new ToolkitModuleLoader();
        var module = loader.Load(new Module_Invalid_WrongEnum(), CreateContext(ModuleAccessOptions_Invalid_NotFlags.None));
        Assert.True(!module.LoadedSuccessfully);
    }

    [Fact]
    public void Load_InvalidModule_WrongEnumType_ReportsErrors()
    {
        var loader = new ToolkitModuleLoader();
        var module = loader.Load(new Module_Valid(), CreateContext(ModuleAccessOptions_ValidOther.None));
        Assert.True(!module.LoadedSuccessfully);
    }

    [Fact]
    public void Load_InvalidModule_ConfigException_ReportsErrors()
    {
        var loader = new ToolkitModuleLoader();
        var module = loader.Load(new Module_Invalid_ConfigException(), CreateContext(ModuleAccessOptions_Valid.None));
        Assert.True(!module.LoadedSuccessfully);
    }

    [Fact]
    public void Load_InvalidModule_InvalidConfig_ReportsErrors()
    {
        var loader = new ToolkitModuleLoader();
        var module = loader.Load(new Module_Invalid_InvalidConfig(), CreateContext(ModuleAccessOptions_Valid.None));
        Assert.True(!module.LoadedSuccessfully);
    }

    [Fact]
    public void Load_InvalidModule_DupedMethods_ReportsErrors()
    {
        var loader = new ToolkitModuleLoader();
        var module = loader.Load(new Module_Invalid_DupeMethods(), CreateContext(ModuleAccessOptions_Valid.None));
        Assert.True(!module.LoadedSuccessfully);
    }

    private static ToolkitModuleContext CreateContext(object accessOptions)
    {
        return new ToolkitModuleContext()
        {
            CurrentRequestModuleAccessOptions = accessOptions
        };
    }
}
