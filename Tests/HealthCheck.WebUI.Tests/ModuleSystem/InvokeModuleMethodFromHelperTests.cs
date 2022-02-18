using HealthCheck.Core.Models;
using HealthCheck.WebUI.Models;
using HealthCheck.WebUI.Tests.Helpers;
using HealthCheck.WebUI.Tests.ModuleSystem.Helpers;
using HealthCheck.WebUI.Util;
using System;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace HealthCheck.WebUI.Tests.ModuleSystem
{
    public class InvokeModuleMethodFromHelperTests
    {
        public ITestOutputHelper Output { get; }

        public InvokeModuleMethodFromHelperTests(ITestOutputHelper output)
        {
            Output = output;
        }

        [Fact]
        public async Task Invoke_ValidMethodThatRequiresNoAccess_HasNoAccessToModule_ReturnsNoAccess()
        {
            var requestInfo = CreateRequestInformation(AccessRoles.None);
            var helper = CreateHelper(AccessRoles.None, (config) =>
            {
            });

            var method = nameof(Module_Valid.ValidMethod_OpenAccess_HasParameterAndContext_NoReturn);
            var jsonPayload = "123";

            var result = await helper.InvokeModuleMethod(requestInfo, typeof(Module_Valid).Name, method, jsonPayload);
            Assert.False(result.HasAccess);
        }

        [Fact]
        public async Task Invoke_ValidMethodThatRequiresNoAccess_HasAccessToModule_ReturnsSuccess()
        {
            var requestInfo = CreateRequestInformation(AccessRoles.Guest);
            var helper = CreateHelper(AccessRoles.Guest, (config) =>
            {
                config.GiveRolesAccessToModule<Module_Valid>(AccessRoles.Guest);
            });

            var method = nameof(Module_Valid.ValidMethod_OpenAccess_HasParameterAndContext_NoReturn);
            var jsonPayload = "123";

            var result = await helper.InvokeModuleMethod(requestInfo, typeof(Module_Valid).Name, method, jsonPayload);
            Assert.True(result.HasAccess);
        }

        [Fact]
        public async Task Invoke_ValidMethodThatRequiresSingleAccess_HasNoAccess_ReturnsNoAccess()
        {
            var requestInfo = CreateRequestInformation(AccessRoles.WebAdmins);
            var helper = CreateHelper(AccessRoles.WebAdmins, (config) =>
            {
                config.GiveRolesAccessToModule<Module_Valid>(AccessRoles.WebAdmins);
            });

            var method = nameof(Module_Valid.ValidMethod_RequiresRead_NoParameter_ReturnsValue);
            var result = await helper.InvokeModuleMethod(requestInfo, typeof(Module_Valid).Name, method, null);
            Assert.False(result.HasAccess);
        }

        [Fact]
        public async Task Invoke_ValidMethodThatRequiresDoubleAccess_HasNoAccess_ReturnsNoAccess()
        {
            var requestInfo = CreateRequestInformation(AccessRoles.WebAdmins);
            var helper = CreateHelper(AccessRoles.WebAdmins, (config) =>
            {
                config.GiveRolesAccessToModule<Module_Valid>(AccessRoles.WebAdmins);
            });

            var method = nameof(Module_Valid.ValidMethod_RequiresReadWrite_NoParameter_ReturnsValue);
            var result = await helper.InvokeModuleMethod(requestInfo, typeof(Module_Valid).Name, method, null);
            Assert.False(result.HasAccess);
        }

        [Fact]
        public async Task Invoke_ValidMethodThatRequiresSingleAccess_HasAccess_ReturnsAccess()
        {
            var requestInfo = CreateRequestInformation(AccessRoles.WebAdmins);
            var helper = CreateHelper(AccessRoles.WebAdmins, (config) =>
            {
                config.GiveRolesAccessToModule(AccessRoles.WebAdmins, ModuleAccessOptions_Valid.Read);
            });

            var method = nameof(Module_Valid.ValidMethod_RequiresRead_NoParameter_ReturnsValue);
            var result = await helper.InvokeModuleMethod(requestInfo, typeof(Module_Valid).Name, method, null);
            Assert.True(result.HasAccess);
        }

        [Fact]
        public async Task Invoke_ValidMethodThatRequiresDoubleAccess_HasOneAccess_ReturnsNoAccess()
        {
            var requestInfo = CreateRequestInformation(AccessRoles.WebAdmins);
            var helper = CreateHelper(AccessRoles.WebAdmins, (config) =>
            {
                config.GiveRolesAccessToModule(AccessRoles.WebAdmins, ModuleAccessOptions_Valid.Read);
            });

            var method = nameof(Module_Valid.ValidMethod_RequiresReadWrite_NoParameter_ReturnsValue);
            var result = await helper.InvokeModuleMethod(requestInfo, typeof(Module_Valid).Name, method, null);
            Assert.False(result.HasAccess);
        }

        [Fact]
        public async Task Invoke_ValidMethodThatRequiresSingleAccess_HasDoubleAccess_ReturnsAccess()
        {
            var requestInfo = CreateRequestInformation(AccessRoles.WebAdmins);
            var helper = CreateHelper(AccessRoles.WebAdmins, (config) =>
            {
                config.GiveRolesAccessToModule(AccessRoles.WebAdmins, ModuleAccessOptions_Valid.Read | ModuleAccessOptions_Valid.Write);
            });

            var method = nameof(Module_Valid.ValidMethod_RequiresRead_NoParameter_ReturnsValue);
            var result = await helper.InvokeModuleMethod(requestInfo, typeof(Module_Valid).Name, method, null);
            Assert.True(result.HasAccess);
        }

        [Fact]
        public async Task Invoke_ValidMethodThatRequiresDoubleAccess_HasDoubleAccess_ReturnsAccess()
        {
            var requestInfo = CreateRequestInformation(AccessRoles.WebAdmins);
            var helper = CreateHelper(AccessRoles.WebAdmins, (config) =>
            {
                config.GiveRolesAccessToModule(AccessRoles.WebAdmins, ModuleAccessOptions_Valid.Read | ModuleAccessOptions_Valid.Write);
            });

            var method = nameof(Module_Valid.ValidMethod_RequiresReadWrite_NoParameter_ReturnsValue);
            var result = await helper.InvokeModuleMethod(requestInfo, typeof(Module_Valid).Name, method, null);
            Assert.True(result.HasAccess);
        }

        private HealthCheckControllerHelper<AccessRoles> CreateHelper(AccessRoles roles, Action<AccessConfig<AccessRoles>> accessConfig)
        {
            var helper = new HealthCheckControllerHelper<AccessRoles>(null, null);
            helper.UseModule(new Module_Valid());
            accessConfig?.Invoke(helper.AccessConfig);
            helper.AfterConfigure(CreateRequestInformation(roles));
            return helper;
        }

        private RequestInformation<AccessRoles> CreateRequestInformation(AccessRoles roles)
            => new RequestInformation<AccessRoles>(roles, "TestUserId", "TestUserName");
    }
}
