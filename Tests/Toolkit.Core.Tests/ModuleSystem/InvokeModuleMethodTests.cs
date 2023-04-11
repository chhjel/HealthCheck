using QoDL.Toolkit.Core.Abstractions.Modules;
using QoDL.Toolkit.Core.Tests.Helpers;
using QoDL.Toolkit.Core.Tests.ModuleSystem.Helpers;
using QoDL.Toolkit.Core.Util.Modules;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace QoDL.Toolkit.Core.Tests.ModuleSystem;

public class InvokeModuleMethodTests
{
    public ITestOutputHelper Output { get; }

    public InvokeModuleMethodTests(ITestOutputHelper output)
    {
        Output = output;
    }

    [Fact]
    public async Task Invoke_ValidMethod_OpenAccess_NoParameter_NoReturn_DoesNotFail()
    {
        var module = LoadValidModuleA();
        var method = module.InvokableMethods.Single(x => x.Name == nameof(Module_Valid.ValidMethod_OpenAccess_NoParameter_NoReturn));
        var context = CreateContext(AccessRoles.Guest, ModuleAccessOptions_Valid.None);
        var serializer = new MockJsonSerializer();
        var result = await method.Invoke(module.Module, context, null, serializer);
        Assert.Null(result);
    }

    [Fact]
    public async Task Invoke_ValidMethod_OpenAccess_HasParameter_NoReturn_DoesNotFail()
    {
        var module = LoadValidModuleA();
        var method = module.InvokableMethods.Single(x => x.Name == nameof(Module_Valid.ValidMethod_OpenAccess_HasParameter_NoReturn));
        var context = CreateContext(AccessRoles.Guest, ModuleAccessOptions_Valid.None);
        var serializer = new MockJsonSerializer(123, "123");
        var result = await method.Invoke(module.Module, context, null, serializer);
        Assert.Null(result);
    }

    [Fact]
    public async Task Invoke_ValidMethod_OpenAccess_HasParameter_ReturnsValue_DoesNotFail()
    {
        var module = LoadValidModuleA();
        var method = module.InvokableMethods.Single(x => x.Name == nameof(Module_Valid.ValidMethod_OpenAccess_HasParameter_ReturnsValue));
        var context = CreateContext(AccessRoles.Guest, ModuleAccessOptions_Valid.None);
        var serializer = new MockJsonSerializer(123, "123");
        await method.Invoke(module.Module, context, null, serializer);
        Assert.Equal(123, serializer.LastSerializedObject);
    }

    [Fact]
    public async Task Invoke_ValidMethod_OpenAccess_HasParameterAndContext_NoReturn_DoesNotFail()
    {
        var module = LoadValidModuleA();
        var method = module.InvokableMethods.Single(x => x.Name == nameof(Module_Valid.ValidMethod_OpenAccess_HasParameterAndContext_NoReturn));
        var context = CreateContext(AccessRoles.Guest, ModuleAccessOptions_Valid.None);
        var serializer = new MockJsonSerializer(123, "123");
        var result = await method.Invoke(module.Module, context, null, serializer);
        Assert.Null(result);
    }

    [Fact]
    public async Task Invoke_ValidMethod_RequiresRead_NoParameter_ReturnsValue_DoesNotFail()
    {
        var module = LoadValidModuleA();
        var method = module.InvokableMethods.Single(x => x.Name == nameof(Module_Valid.ValidMethod_RequiresRead_NoParameter_ReturnsValue));
        var context = CreateContext(AccessRoles.Guest, ModuleAccessOptions_Valid.None);
        var serializer = new MockJsonSerializer(null, null);
        var result = await method.Invoke(module.Module, context, null, serializer);
        Assert.Null(result);
    }

    [Fact]
    public async Task Invoke_ValidMethod_RequiresReadWrite_NoParameter_ReturnsValue_DoesNotFail()
    {
        var module = LoadValidModuleA();
        var method = module.InvokableMethods.Single(x => x.Name == nameof(Module_Valid.ValidMethod_RequiresReadWrite_NoParameter_ReturnsValue));
        var context = CreateContext(AccessRoles.Guest, ModuleAccessOptions_Valid.None);
        var serializer = new MockJsonSerializer(null, null);
        var result = await method.Invoke(module.Module, context, null, serializer);
        Assert.Null(result);
    }

    [Fact]
    public async Task Invoke_ValidMethodAsync_OpenAccess_NoParameter_NoReturn_DoesNotFail()
    {
        var module = LoadValidModuleA();
        var method = module.InvokableMethods.Single(x => x.Name == nameof(Module_Valid.ValidMethodAsync_OpenAccess_NoParameter_NoReturn));
        var context = CreateContext(AccessRoles.Guest, ModuleAccessOptions_Valid.None);
        var serializer = new MockJsonSerializer(null, null);
        var result = await method.Invoke(module.Module, context, null, serializer);
        Assert.Null(result);
    }

    [Fact]
    public async Task Invoke_ValidMethodAsync_OpenAccess_NoParameter_ReturnsValue_DoesNotFail()
    {
        var module = LoadValidModuleA();
        var method = module.InvokableMethods.Single(x => x.Name == nameof(Module_Valid.ValidMethodAsync_OpenAccess_NoParameter_ReturnsValue));
        var context = CreateContext(AccessRoles.Guest, ModuleAccessOptions_Valid.None);
        var serializer = new MockJsonSerializer(123, "123");
        await method.Invoke(module.Module, context, null, serializer);
        Assert.Equal(123, serializer.LastSerializedObject);
    }

    public static ToolkitLoadedModule LoadValidModuleA()
    {
        var loader = new ToolkitModuleLoader();
        return loader.Load(new Module_Valid(), CreateContext(ModuleAccessOptions_Valid.None));
    }

    private static ToolkitModuleContext CreateContext(object accessOptions)
    {
        return new ToolkitModuleContext()
        {
            CurrentRequestModuleAccessOptions = accessOptions
        };
    }

    public static ToolkitModuleContext CreateContext(object accessRoles, object accessOptions)
    {
        return new ToolkitModuleContext()
        {
            UserId = "TestId",
            UserName = "Test Username",
            ModuleName = "Test Module",

            CurrentRequestRoles = accessRoles,
            CurrentRequestModuleAccessOptions = accessOptions
        };
    }
}
