using System.IO;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;
using HealthCheck.Core.Util;
using HealthCheck.DevTest._TestImplementation;
using HealthCheck.WebUI.Models;
using HealthCheck.WebUI.ViewModels;
using HealthCheck.WebUI.Abstractions;
using HealthCheck.Core.Services.SiteStatus;
using System;
using HealthCheck.Core.Entities;
using HealthCheck.Core.Enums;
using HealthCheck.Core.Extensions;
using System.Collections.Generic;

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
                _siteStatusService = CreateSiteStatusService();
            }
            SiteStatusService = _siteStatusService;

            if (addSomeRandomEvents)
            {
                ResetEvents(false);
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

        protected override void Configure(HttpRequestBase request)
        {
            TestRunner.IncludeExceptionStackTraces = CurrentRequestAccessRoles.HasValue && CurrentRequestAccessRoles.Value.HasFlag(RuntimeTestAccessRole.SystemAdmins);
            AccessOptions.OverviewPageAccess = new Maybe<RuntimeTestAccessRole>(RuntimeTestAccessRole.WebAdmins);
            //AccessOptions.TestsPageAccess = new Maybe<RuntimeTestAccessRole>(RuntimeTestAccessRole.SystemAdmins);
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

        public FileResult GetScript()
        {
            var filepath = Path.GetFullPath($@"{HostingEnvironment.MapPath("~")}..\HealthCheck.Frontend\dist\healthcheckfrontend.js");
            return new FileStreamResult(new FileStream(filepath, FileMode.Open), "content-disposition");
        }
        
        public ActionResult ResetEvents(bool reInitService = true)
        {
            if (reInitService)
            {
                SiteStatusService = CreateSiteStatusService();
            }

            for (int i = 0; i < 20; i++)
            {
                AddEvent();
            }
            return Content("Mock events reset");
        }

        private static readonly Random _rand = new Random();
        public ActionResult AddEvent()
        {
            if (!Enabled || SiteStatusService == null) return HttpNotFound();

            CreateSomeData(out string title, out string description);

            var ev = new SiteEvent()
            {
                EventTypeId = $"Error type {_rand.Next(10000)}",
                Title = title,
                Description = description,
                Timestamp = DateTime.Now
                    .AddDays(-7 + _rand.Next(7))
                    .AddMinutes(_rand.Next(0, 24 * 60)),
                Severity = (_rand.Next(100) < 10) ? SiteEventSeverity.Fatal
                            : (_rand.Next(100) < 25) ? SiteEventSeverity.Error
                                : (_rand.Next(100) < 50) ? SiteEventSeverity.Warning : SiteEventSeverity.Information,
                RelatedLinks = new List<HyperLink>()
                {
                    new HyperLink("Page that failed", "https://www.google.com?etc"),
                    new HyperLink("Error log", "https://www.google.com?q=errorlog"),
                }
            };
            SiteStatusService.RegisterEvent(ev);
            return CreateJsonResult(ev);
        }

        private SiteStatusService CreateSiteStatusService()
            => new SiteStatusService(new MemorySiteStatusStorageService());

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
        private readonly string[] _accidents = new[] { "is on fire", "exploded", "is slow", "decided to close", "is infected with ransomware", "is not happy" };
        private readonly string[] _reactions = new[] { "on fire", "not pleased", "confused", "not happy", "leaving" };
    }
}
