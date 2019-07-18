using System.IO;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;
using HealthCheck.Core.Util;
using HealthCheck.DevTest._TestImplementation;
using HealthCheck.WebUI.Models;
using HealthCheck.WebUI.ViewModels;
using HealthCheck.WebUI.Abstractions;
using System;
using HealthCheck.Core.Entities;
using HealthCheck.Core.Enums;
using HealthCheck.Core.Extensions;
using HealthCheck.WebUI.Services;
using HealthCheck.Core.Abstractions;
using System.Threading.Tasks;
using System.Linq;
using HealthCheck.Core.Services;
using System.Collections.Generic;

namespace HealthCheck.DevTest.Controllers
{
    public class DevController : HealthCheckControllerBase<RuntimeTestAccessRole>
    {
        private const string EndpointBase = "/dev";
        private static SiteEventService _siteEventService;
        private static IAuditEventStorage _auditEventService;

        #region Init
        public DevController()
            : base(assemblyContainingTests: typeof(DevController).Assembly) {

            if (_siteEventService == null)
            {

                _siteEventService = CreateSiteEventService();
                _auditEventService = CreateAuditEventService();
            }

            SiteEventService = _siteEventService;
            AuditEventService = _auditEventService;

            if (!_hasInited)
            {
                InitOnce();
            }
        }

        private SiteEventService CreateSiteEventService()
            => new SiteEventService(new FlatFileSiteEventStorage(HostingEnvironment.MapPath("~/App_Data/SiteEventStorage.json"),
                maxEventAge: TimeSpan.FromDays(5), delayFirstCleanup: false));

        private IAuditEventStorage CreateAuditEventService()
            => new FlatFileAuditEventStorage(HostingEnvironment.MapPath("~/App_Data/AuditEventStorage.json"),
                maxEventAge: TimeSpan.FromDays(30), delayFirstCleanup: false);

        private static bool _hasInited = false;
        private void InitOnce()
        {
            _hasInited = true;
            Task.Run(() => AddEvents(false));
        }
        #endregion

        #region Overrides
        protected override FrontEndOptionsViewModel GetFrontEndOptions()
            => new FrontEndOptionsViewModel(EndpointBase)
            {
                ApplicationTitle = "Test Monitor",
                PagePriority = new List<HealthCheckPageType>()
                {
                    HealthCheckPageType.Tests,
                    HealthCheckPageType.Overview,
                    HealthCheckPageType.AuditLog,
                }
            };

        protected override PageOptions GetPageOptions()
            => new PageOptions()
            {
                JavaScriptUrls = new List<string> {
                    $"{EndpointBase}/GetVendorScript",
                    $"{EndpointBase}/GetMainScript",
                },
                PageTitle = "Test Monitor"
            };

        protected override void Configure(HttpRequestBase request)
        {
            TestRunner.IncludeExceptionStackTraces = CurrentRequestAccessRoles.HasValue && CurrentRequestAccessRoles.Value.HasFlag(RuntimeTestAccessRole.SystemAdmins);
            AccessOptions.OverviewPageAccess = new Maybe<RuntimeTestAccessRole>(RuntimeTestAccessRole.Guest);
            AccessOptions.TestsPageAccess = new Maybe<RuntimeTestAccessRole>(RuntimeTestAccessRole.WebAdmins);
            AccessOptions.AuditLogAccess = new Maybe<RuntimeTestAccessRole>(RuntimeTestAccessRole.SystemAdmins);
            AccessOptions.InvalidTestsAccess = new Maybe<RuntimeTestAccessRole>(RuntimeTestAccessRole.SomethingElse);
        }

        protected override void SetTestSetGroupsOptions(TestSetGroupsOptions options)
        {
            options
            .SetOptionsFor(RuntimeTestConstants.Group.AdminStuff, uiOrder: 100)
            .SetOptionsFor(RuntimeTestConstants.Group.BottomGroup, uiOrder: -100);
        }

        protected override RequestInformation<RuntimeTestAccessRole> GetRequestInformation(HttpRequestBase request)
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

            return new RequestInformation<RuntimeTestAccessRole>(roles, "dev42", "Dev user");
        }
        #endregion

        #region dev
        public FileResult GetMainScript()
        {
            var filepath = Path.GetFullPath($@"{HostingEnvironment.MapPath("~")}..\HealthCheck.Frontend\dist\healthcheck.js");
            return new FileStreamResult(new FileStream(filepath, FileMode.Open), "content-disposition");
        }

        [OutputCache(Duration = 1200, VaryByParam = "none")]
        public FileResult GetVendorScript()
        {
            var filepath = Path.GetFullPath($@"{HostingEnvironment.MapPath("~")}..\HealthCheck.Frontend\dist\healthcheck.vendor.js");
            return new FileStreamResult(new FileStream(filepath, FileMode.Open), "content-disposition");
        }

        public async Task<ActionResult> RunHealthChecks()
        {
            var results = await TestRunner.ExecuteTests(TestDiscoverer, 
                testFilter: (test) => test.Categories.Contains(RuntimeTestConstants.Categories.ScheduledHealthCheck),
                siteEventService: SiteEventService,
                auditEventService: AuditEventService
            );
            return CreateJsonResult(results.Select(x => new { Test = x.Test?.Name, Result = x.Message, SiteEventTitle = x.SiteEvent?.Title }));
        }

        // New mock data
        public async Task<ActionResult> AddEvents(bool reInitService = true)
        {
            if (reInitService)
            {
                SiteEventService = CreateSiteEventService();
            }

            if ((await SiteEventService.GetEvents(DateTime.MinValue, DateTime.MaxValue)).Count == 0)
            {
                for (int i = 0; i < 20; i++)
                {
                    await AddEvent();
                }
                return Content("Mock events reset");
            }
            else
            {
                return Content("Already have some mock events in place");
            }
        }

        // New mock data
        private static readonly Random _rand = new Random();
        public async Task<ActionResult> AddEvent()
        {
            if (!Enabled || SiteEventService == null) return HttpNotFound();

            CreateSomeData(out string title, out string description);
            var severity = (_rand.Next(100) < 10) ? SiteEventSeverity.Fatal
                            : (_rand.Next(100) < 25) ? SiteEventSeverity.Error
                                : (_rand.Next(100) < 50) ? SiteEventSeverity.Warning : SiteEventSeverity.Information;

            var ev = new SiteEvent(
                severity, $"Error type {_rand.Next(10000)}",
                title, description,
                duration: _rand.Next(1, 90)
            ) {
                Timestamp = DateTime.Now
                    .AddDays(-7 + _rand.Next(7))
                    .AddMinutes(_rand.Next(0, 24 * 60))
            }
            .AddRelatedLink("Page that failed", "https://www.google.com?etc")
            .AddRelatedLink("Error log", "https://www.google.com?q=errorlog");

            await SiteEventService.StoreEvent(ev);
            return CreateJsonResult(ev);
        }

        private string AddXFix(string subject, string xfix)
        {
            if (xfix.Contains("|"))
            {
                var parts = xfix.Split('|');
                var prefix = parts[0];
                var suffix = parts[1];
                return $"{prefix} {subject}{suffix}";
            } else
            {
                return $"{xfix}{subject}";
            }
        }

        private void CreateSomeData(out string title, out string description)
        {
            var subject = _subjects.RandomElement(_rand);
            subject = AddXFix(subject, _subjectXFixes.RandomElement(_rand));
            var accident = _accidents.RandomElement(_rand);
            var reaction = _reactions.RandomElement(_rand);
            var reactor = _subjects.RandomElement(_rand);
            reactor = AddXFix(reactor, _subjectXFixes.RandomElement(_rand));

            title = $"{subject} {accident}".CapitalizeFirst();
            description = $"{subject} {accident} and {reactor} is {reaction}.".CapitalizeFirst(); ;
        }

        private readonly string[] _subjectXFixes = new[] { "the ", "an unknown ", "most of the |s", "several of the |s", "one of the |s" };
        private readonly string[] _subjects = new[] { "service", "server", "integration", "frontpage", "developer", "codebase", "project manager", "CEO" };
        private readonly string[] _accidents = new[] { "is on fire", "exploded", "is slow", "decided to close", "is infected with ransomware", "is not happy", "don't know what to do" };
        private readonly string[] _reactions = new[] { "on fire", "not pleased", "confused", "not happy", "leaving" };
        #endregion
    }
}
