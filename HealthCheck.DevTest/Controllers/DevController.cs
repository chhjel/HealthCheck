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

        protected override void Configure(HttpRequestBase request)
        {
            TestRunner.IncludeExceptionStackTraces = CurrentRequestAccessRoles.HasValue && CurrentRequestAccessRoles.Value.HasFlag(RuntimeTestAccessRole.SystemAdmins);
        }

        protected override void SetTestSetGroupsOptions(TestSetGroupsOptions options)
        {
            options
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

            CreateSomeData(out string title, out string description);

            var ev = new SiteEvent()
            {
                EventTypeId = $"Error type {_rand.Next(10000)}",
                Title = title,
                Description = description,
                Timestamp = DateTime.Now.AddDays(-7 + _rand.Next(14)),
                Severity = (_rand.Next(100) > 90) ? SiteEventSeverity.Fatal
                    : (_rand.Next(100) > 50) ? SiteEventSeverity.Error
                        : (_rand.Next(100) > 80) ? SiteEventSeverity.Information : SiteEventSeverity.Warning
            };
            SiteStatusService.RegisterEvent(ev);
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
        private readonly string[] _accidents = new[] { "is on fire", "exploded", "is slow", "decided to close", "is infected with ransomware", "is not happy" };
        private readonly string[] _reactions = new[] { "on fire", "not pleased", "confused", "not happy", "leaving" };
    }
}
