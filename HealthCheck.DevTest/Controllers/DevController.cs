using System.IO;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;
using HealthCheck.Core.Services;
using HealthCheck.Core.Util;
using HealthCheck.DevTest._TestImplementation;
using HealthCheck.WebUI.Models;
using HealthCheck.WebUI.ViewModels;
using HealthCheck.WebUI.Abstractions;
using HealthCheck.Core.Services.SiteStatus;
using System;
using HealthCheck.Core.Entities;
using HealthCheck.Core.Enums;

namespace HealthCheck.DevTest.Controllers
{
    public class DevController : HealthCheckControllerBase<RuntimeTestAccessRole>
    {
        private const string EndpointBase = "/dev";
        private static SiteStatusService _siteStatusService;

        public DevController()
            : base(assemblyContainingTests: typeof(DevController).Assembly) {

            var addSomeRandomEvents = false;
            if (_siteStatusService == null)
            {
                addSomeRandomEvents = true;
                _siteStatusService = new SiteStatusService(new MemorySiteStatusStorageService());
            }
            SiteStatusService = _siteStatusService;

            if (addSomeRandomEvents)
            {
                for (int i = 0; i < 20; i++)
                {
                    AddEvent();
                }
            }
        }


        #region Overrides
        protected override FrontEndOptionsViewModel GetFrontEndOptions()
            => new FrontEndOptionsViewModel(EndpointBase)
            {
                ApplicationTitle = "Test Monitor"
            };

        protected override PageOptions GetPageOptions()
            => new PageOptions()
            {
                JavaScriptUrl = $"{EndpointBase}/GetScript",
                PageTitle = "Test Monitor"
            };

        protected override void SetOptionalOptions(HttpRequestBase request, TestRunnerService testRunner, TestDiscoveryService testDiscoverer)
        {
            var requestRoles = GetRequestAccessRoles(request);
            testRunner.IncludeExceptionStackTraces = requestRoles.HasValue && requestRoles.Value.HasFlag(RuntimeTestAccessRole.SystemAdmins);
            testDiscoverer.GroupOptions
                .SetOptionsFor(RuntimeTestConstants.Group.AdminStuff, uiOrder: 100)
                .SetOptionsFor(RuntimeTestConstants.Group.BottomGroup, uiOrder: -100);
        }

        protected override Maybe<RuntimeTestAccessRole> GetRequestAccessRoles(HttpRequestBase request)
        {
            var roles = RuntimeTestAccessRole.Guest;

            if (request.QueryString["webadmin"] != null)
            {
                roles |= RuntimeTestAccessRole.WebAdmins;
            }
            if (request.QueryString["sysadmin"] != null)
            {
                roles |= RuntimeTestAccessRole.SystemAdmins;
            }

            return new Maybe<RuntimeTestAccessRole>(roles);
        }
        #endregion

        public FileResult GetScript()
        {
            var filepath = Path.GetFullPath($@"{HostingEnvironment.MapPath("~")}..\HealthCheck.Frontend\dist\healthcheckfrontend.js");
            return new FileStreamResult(new FileStream(filepath, FileMode.Open), "content-disposition");
        }

        private static readonly Random _rand = new Random();
        public virtual ActionResult AddEvent()
        {
            if (!Enabled || SiteStatusService == null) return HttpNotFound();

            var ev = new SiteEvent()
            {
                EventTypeId = $"Error type {_rand.Next(10000)}",
                Title = "Something integration error",
                Description = "Something happened in the integration with something",
                Timestamp = DateTime.Now.AddDays(-7 + _rand.Next(14)),
                Severity = (_rand.Next(100) > 90) ? SiteEventSeverity.Fatal
                    : (_rand.Next(100) > 50) ? SiteEventSeverity.Error : SiteEventSeverity.Warning
            };
            SiteStatusService.RegisterEvent(ev);
            return CreateJsonResult(ev);
        }
    }
}
