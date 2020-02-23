using HealthCheck.Core.Attributes;
using HealthCheck.Core.Entities;
using HealthCheck.Core.Services;
using HealthCheck.Core.Services.Storage;
using HealthCheck.Core.Util;
using HealthCheck.WebUI.Models;
using HealthCheck.WebUI.Tests.Helpers;
using HealthCheck.WebUI.Util;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace HealthCheck.WebUI.Tests
{
    public class HealthCheckControllerHelperTests
    {
        public ITestOutputHelper Output { get; }

        public HealthCheckControllerHelperTests(ITestOutputHelper output)
        {
            Output = output;
        }

        [Theory]
        [InlineData(AccessRoles.None)]
        [InlineData(AccessRoles.Guest)]
        [InlineData(AccessRoles.WebAdmins)]
        [InlineData(AccessRoles.SystemAdmins)]
        [InlineData(AccessRoles.WebAdmins | AccessRoles.SystemAdmins)]
        [InlineData(AccessRoles.Everyone)]
        public async Task GetSiteEventsViewModel_DoesNotFail(AccessRoles roles)
        {
            var helper = CreateHelper<AccessRoles>();
            var siteEventStorage = new MemorySiteEventStorage();
            var siteEventService = new SiteEventService(siteEventStorage);
            helper.Services.SiteEventService = siteEventService;
            await siteEventService.StoreEvent(new SiteEvent(Core.Enums.SiteEventSeverity.Error, "typeId", "Title", "Desc", 17));

            var models = await helper.GetSiteEventsViewModel(new Maybe<AccessRoles>(roles));
            Assert.NotEmpty(models);
        }

        [Theory]
        [InlineData(AccessRoles.None)]
        [InlineData(AccessRoles.Guest)]
        [InlineData(AccessRoles.WebAdmins)]
        [InlineData(AccessRoles.SystemAdmins)]
        [InlineData(AccessRoles.WebAdmins | AccessRoles.SystemAdmins)]
        [InlineData(AccessRoles.Everyone)]
        public void GetRequestLogActions_DoesNotFail(AccessRoles roles)
        {
            var helper = CreateHelper<AccessRoles>();
            var models = helper.GetRequestLogActions(new Maybe<AccessRoles>(roles));
            Assert.NotNull(models);
        }

        [Theory]
        [InlineData(AccessRoles.None)]
        [InlineData(AccessRoles.Guest)]
        [InlineData(AccessRoles.WebAdmins)]
        [InlineData(AccessRoles.SystemAdmins)]
        [InlineData(AccessRoles.WebAdmins | AccessRoles.SystemAdmins)]
        [InlineData(AccessRoles.Everyone)]
        public void GetTestDefinitionsViewModel_DoesNotFail(AccessRoles roles)
        {
            var helper = CreateHelper<AccessRoles>();
            var model = helper.GetTestDefinitionsViewModel(new Maybe<AccessRoles>(roles));
            Assert.NotEmpty(model.TestSets);
        }

        [Theory]
        [InlineData(AccessRoles.WebAdmins)]
        [InlineData(AccessRoles.WebAdmins | AccessRoles.SystemAdmins)]
        public async Task GetAuditEventsFilterViewModel_DoesNotFail(AccessRoles roles)
        {
            var helper = CreateHelper<AccessRoles>();
            helper.AccessOptions.AuditLogAccess = new Maybe<AccessRoles>(AccessRoles.WebAdmins);
            var auditEventService = new MemoryAuditEventStorage();
            helper.Services.AuditEventService = auditEventService;
            await auditEventService.StoreEvent(
                new AuditEvent(DateTime.Now, Core.Enums.AuditEventArea.Tests, "Title", "Subject", "123", "User 123", EnumUtils.GetFlaggedEnumValues(roles).Select(x => x.ToString()).ToList())
            );

            var models = await helper.GetAuditEventsFilterViewModel(new Maybe<AccessRoles>(roles), new Models.AuditEventFilterInputData());
            Assert.NotEmpty(models);
        }

        private HealthCheckControllerHelper<AccessRoles> CreateHelper<AccessRoles>()
        {
            var helper = new HealthCheckControllerHelper<AccessRoles>(new HealthCheckServiceContainer<AccessRoles>());
            helper.TestDiscoverer.AssemblyContainingTests = GetType().Assembly;
            return helper;
        }

        [RuntimeTestClass(Id = "TestSetIdA2", Description = "Some test set #2", Name = "Dev test set #2", DefaultCategory = "TestSetId2Category")]
        public class TestClassX
        {
            [RuntimeTest(Name = "TestMethodA2")]
            public async Task<TestResult> TestMethodA2(string stringArg = "wut", bool boolArg = true, int intArg = 123)
            {
                await Task.Delay(1);
                return TestResult.CreateSuccess($"Success! [{stringArg}, {boolArg}, {intArg}]");
            }

            [RuntimeTest(Name = "TestMethodB2", Category = "TestMethodB2Category")]
            public async Task<TestResult> TestMethodB2()
            {
                await Task.Delay(1);
                return TestResult.CreateSuccess($"Success!");
            }
        }
    }
}
