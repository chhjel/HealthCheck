using HealthCheck.Core.Entities;
using HealthCheck.Core.Services;
using HealthCheck.Core.Services.Storage;
using HealthCheck.Core.Util;
using HealthCheck.WebUI.Abstractions;
using HealthCheck.WebUI.Models;
using HealthCheck.WebUI.Tests.Helpers;
using HealthCheck.WebUI.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace HealthCheck.WebUI.Tests
{
    public class HealthCheckControllerDotNetTests
    {
        public ITestOutputHelper Output { get; }

        public HealthCheckControllerDotNetTests(ITestOutputHelper output)
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
        public async Task GetSiteEvents_DoesNotFail(AccessRoles roles)
        {
            var controller = await CreateController(roles);
            var result = await controller.GetSiteEvents();
            Assert.NotNull(result);

            Assert.IsType<ContentResult>(result);
            var content = (ContentResult)result;
            var contentBody = content.Content;
            Output.WriteLine(contentBody);
            Assert.Contains("eventTypeA", contentBody);
        }

        [Theory]
        [InlineData(AccessRoles.None)]
        [InlineData(AccessRoles.Guest)]
        [InlineData(AccessRoles.WebAdmins)]
        [InlineData(AccessRoles.SystemAdmins)]
        [InlineData(AccessRoles.WebAdmins | AccessRoles.SystemAdmins)]
        [InlineData(AccessRoles.Everyone)]
        public async Task GetTests_DoesNotFail(AccessRoles roles)
        {
            var controller = await CreateController(roles);
            var result = controller.GetTests();
            Assert.NotNull(result);

            Assert.IsType<ContentResult>(result);
            var content = (ContentResult)result;
            var contentBody = content.Content;
            Output.WriteLine(contentBody);
            Assert.Contains("TestSetIdA2", contentBody);
        }

        [Theory]
        [InlineData(AccessRoles.WebAdmins)]
        [InlineData(AccessRoles.WebAdmins | AccessRoles.SystemAdmins)]
        [InlineData(AccessRoles.Everyone)]
        public async Task GetFilteredAudits_WithAccess_DoesNotFail(AccessRoles roles)
        {
            var controller = await CreateController(roles);
            var result = await controller.GetFilteredAudits();
            Assert.NotNull(result);

            Assert.IsType<ContentResult>(result);
            var content = (ContentResult)result;
            var contentBody = content.Content;
            Output.WriteLine(contentBody);
            Assert.Contains("User 123", contentBody);
        }

        [Theory]
        [InlineData(AccessRoles.None)]
        [InlineData(AccessRoles.Guest)]
        [InlineData(AccessRoles.SystemAdmins)]
        public async Task GetFilteredAudits_WithoutAccess_Gives404(AccessRoles roles)
        {
            var controller = await CreateController(roles);
            var result = await controller.GetFilteredAudits();
            Assert.NotNull(result);

            Assert.IsType<NotFoundResult>(result);
        }

        private async Task<HealthCheckControllerDotNet> CreateController(AccessRoles allowedRoles)
        {
            var controller = new HealthCheckControllerDotNet(allowedRoles);
            await controller.InitTestData();
            return controller;
        }
    }

    public class HealthCheckControllerDotNet : HealthCheckControllerBase<AccessRoles>
    {
        private AccessRoles AllowedRoles { get; set; }

        public HealthCheckControllerDotNet(AccessRoles allowedRoles)
            : base(assemblyContainingTests: typeof(HealthCheckControllerDotNet).Assembly)
        {
            SiteEventService = new SiteEventService(new MemorySiteEventStorage());
            AuditEventService = new MemoryAuditEventStorage();

            AllowedRoles = allowedRoles;
            AccessOptions.AuditLogAccess = new Maybe<AccessRoles>(AccessRoles.WebAdmins);
        }

        public async Task InitTestData()
        {
            InitRequestAsync();
            await SiteEventService.StoreEvent(new SiteEvent(Core.Enums.SiteEventSeverity.Error, "eventTypeA", "TitleA", "DescA", 12));
            await AuditEventService.StoreEvent(
                new AuditEvent(DateTime.Now, Core.Enums.AuditEventArea.Tests, "Title", "Subject", "123", "User 123", EnumUtils.GetFlaggedEnumValues(AllowedRoles).Select(x => x.ToString()).ToList())
            );
        }

        public void InitRequestAsync()
        {
            CurrentRequestInformation = GetRequestInformation(null);
            CurrentRequestAccessRoles = CurrentRequestInformation?.AccessRole;
            Configure(null);
        }

        protected override FrontEndOptionsViewModel GetFrontEndOptions() => new FrontEndOptionsViewModel("/HealthCheck");
        protected override PageOptions GetPageOptions() => new PageOptions();
        protected override RequestInformation<AccessRoles> GetRequestInformation(HttpRequest request) => new RequestInformation<AccessRoles>(AllowedRoles);
    }
}
