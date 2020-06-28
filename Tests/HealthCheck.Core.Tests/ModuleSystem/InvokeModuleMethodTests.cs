using HealthCheck.Core.Abstractions.Modules;
using HealthCheck.Core.Tests.Helpers;
using HealthCheck.Core.Tests.ModuleSystem.Helpers;
using HealthCheck.Core.Util;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using static HealthCheck.Core.Util.HealthCheckModuleLoader;

namespace HealthCheck.Core.Tests.ModuleSystem
{
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

        public static HealthCheckLoadedModule LoadValidModuleA()
        {
            var loader = new HealthCheckModuleLoader();
            return loader.Load(new Module_Valid(), CreateContext(ModuleAccessOptions_Valid.None));
        }

        private static HealthCheckModuleContext CreateContext(object accessOptions)
        {
            return new HealthCheckModuleContext()
            {
                CurrentRequestModuleAccessOptions = accessOptions
            };
        }

        public static HealthCheckModuleContext CreateContext(object accessRoles, object accessOptions)
        {
            return new HealthCheckModuleContext()
            {
                UserId = "TestId",
                UserName = "Test Username",
                ModuleName = "Test Module",

                CurrentRequestRoles = accessRoles,
                CurrentRequestModuleAccessOptions = accessOptions
            };
        }
    }
}
