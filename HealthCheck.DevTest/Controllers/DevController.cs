using HealthCheck.Core.Abstractions;
using HealthCheck.Core.Attributes;
using HealthCheck.Core.Entities;
using HealthCheck.Core.Enums;
using HealthCheck.Core.Extensions;
using HealthCheck.Core.Modules.Dataflow;
using HealthCheck.Core.Modules.Diagrams.FlowCharts;
using HealthCheck.Core.Modules.Diagrams.SequenceDiagrams;
using HealthCheck.Core.Services;
using HealthCheck.Core.Services.Models;
using HealthCheck.Core.Util;
using HealthCheck.DevTest._TestImplementation;
using HealthCheck.DevTest._TestImplementation.Dataflow;
using HealthCheck.RequestLog.Services;
using HealthCheck.WebUI.Abstractions;
using HealthCheck.WebUI.Models;
using HealthCheck.WebUI.Services;
using HealthCheck.WebUI.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;

namespace HealthCheck.DevTest.Controllers
{
    public class DevController : HealthCheckControllerBase<RuntimeTestAccessRole>
    {
        private const string EndpointBase = "/dev";
        private static ISiteEventService _siteEventService;
        private static IAuditEventStorage _auditEventService;
        private static readonly TestStreamA testStreamA = new TestStreamA();
        private static readonly TestStreamB testStreamB = new TestStreamB();
        private static readonly TestStreamC testStreamC = new TestStreamC();
        private static readonly SimpleStream simpleStream = new SimpleStream("Simple A");

        #region Init
        public DevController()
            : base(assemblyContainingTests: typeof(DevController).Assembly) {

            if (_siteEventService == null)
            {

                _siteEventService = CreateSiteEventService();
                _auditEventService = CreateAuditEventService();
            }

            simpleStream.InsertEntries(new[] {
                GenericDataflowStreamObject.Create(new TestEntry() { Code = "6235235", Name = "Name A" }),
                GenericDataflowStreamObject.Create(new TestEntry() { Code = "1234", Name = "Name B" }),
                GenericDataflowStreamObject.Create(new TestEntry() { Code = "235235", Name = "Name C" })
            });

            Services.SiteEventService = _siteEventService;
            Services.AuditEventService = _auditEventService;
            Services.LogSearcherService = CreateLogSearcherService();
            Services.RequestLogService = RequestLogServiceAccessor.Current;
            Services.SequenceDiagramService = new DefaultSequenceDiagramService(new DefaultSequenceDiagramServiceOptions()
            {
                DefaultSourceAssemblies = new[] { typeof(DevController).Assembly }
            });
            Services.FlowChartsService = new DefaultFlowChartService(new DefaultFlowChartServiceOptions()
            {
                DefaultSourceAssemblies = new[] { typeof(DevController).Assembly }
            });
            Services.DataflowService = new DefaultDataflowService<RuntimeTestAccessRole>(new DefaultDataflowServiceOptions<RuntimeTestAccessRole>()
            {
                Streams = new IDataflowStream<RuntimeTestAccessRole>[]
                {
                    testStreamA,
                    testStreamB,
                    testStreamC,
                    simpleStream
                }
            });

            if (!_hasInited)
            {
                InitOnce();
            }
        }

        private ILogSearcherService CreateLogSearcherService()
            => new FlatFileLogSearcherService(new FlatFileLogSearcherServiceOptions()
                    .IncludeLogFilesInDirectory(HostingEnvironment.MapPath("~/App_Data/TestLogs/")));

        private ISiteEventService CreateSiteEventService()
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
                ApplicationTitleLink = "/?sysadmin=x&webadmin=1",
                PagePriority = new List<HealthCheckPageType>()
                {
                    HealthCheckPageType.Tests,
                    HealthCheckPageType.Overview,
                    HealthCheckPageType.RequestLog,
                    HealthCheckPageType.Dataflow,
                    HealthCheckPageType.Documentation,
                    HealthCheckPageType.LogViewer,
                    HealthCheckPageType.AuditLog
                },
                ApplyCustomColumnRuleByDefault = true,
                EnableDiagramSandbox = true,
                EnableDiagramDetails = true
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
            AccessOptions.DocumentationPageAccess = new Maybe<RuntimeTestAccessRole>(RuntimeTestAccessRole.WebAdmins);
            AccessOptions.TestsPageAccess = new Maybe<RuntimeTestAccessRole>(RuntimeTestAccessRole.WebAdmins | RuntimeTestAccessRole.API);
            AccessOptions.AuditLogAccess = new Maybe<RuntimeTestAccessRole>(RuntimeTestAccessRole.SystemAdmins);
            AccessOptions.LogViewerPageAccess = new Maybe<RuntimeTestAccessRole>(RuntimeTestAccessRole.SystemAdmins);
            AccessOptions.InvalidTestsAccess = new Maybe<RuntimeTestAccessRole>(RuntimeTestAccessRole.SystemAdmins);
            AccessOptions.SiteEventDeveloperDetailsAccess = new Maybe<RuntimeTestAccessRole>(RuntimeTestAccessRole.SystemAdmins);
            AccessOptions.RequestLogPageAccess = new Maybe<RuntimeTestAccessRole>(RuntimeTestAccessRole.SystemAdmins);
            AccessOptions.ClearRequestLogAccess = new Maybe<RuntimeTestAccessRole>(RuntimeTestAccessRole.SystemAdmins);
            AccessOptions.PingAccess = new Maybe<RuntimeTestAccessRole>(RuntimeTestAccessRole.API);
            AccessOptions.DataflowPageAccess = new Maybe<RuntimeTestAccessRole>(RuntimeTestAccessRole.WebAdmins);
        }

        protected override void SetTestSetGroupsOptions(TestSetGroupsOptions options)
        {
            options
                .SetOptionsFor(RuntimeTestConstants.Group.AdminStuff, uiOrder: 100)
                .SetOptionsFor(RuntimeTestConstants.Group.AlmostTopGroup, uiOrder: 50)
                .SetOptionsFor(RuntimeTestConstants.Group.AlmostBottomGroup, uiOrder: -20)
                .SetOptionsFor(RuntimeTestConstants.Group.BottomGroup, uiOrder: -50);
        }

        protected override RequestInformation<RuntimeTestAccessRole> GetRequestInformation(HttpRequestBase request)
        {
            var roles = RuntimeTestAccessRole.Guest;
            if (request.QueryString["noaccess"] != null)
            {
                roles = RuntimeTestAccessRole.None;
                return new RequestInformation<RuntimeTestAccessRole>(roles, "no_access_test", "No user");
            }

            roles |= RuntimeTestAccessRole.SystemAdmins | RuntimeTestAccessRole.WebAdmins;
            if (request.QueryString["notsysadmin"] != null)
            {
                roles &= ~RuntimeTestAccessRole.SystemAdmins;
            }

            //if (request.QueryString["webadmin"] != null)
            //{
            //    roles |= RuntimeTestAccessRole.WebAdmins;
            //}
            //if (request.QueryString["sysadmin"] != null)
            //{
            //    roles |= RuntimeTestAccessRole.SystemAdmins;
            //}

            if (request.QueryString["key"] == "test")
            {
                roles |= RuntimeTestAccessRole.API;
                return new RequestInformation<RuntimeTestAccessRole>(roles, "apitest", "API User");
            }

            return new RequestInformation<RuntimeTestAccessRole>(roles, "dev42", "Dev user");
        }
        #endregion

        #region dev
        [RequestLogInfo(hide: true)]
        public FileResult GetMainScript()
        {
            var filepath = Path.GetFullPath($@"{HostingEnvironment.MapPath("~")}..\HealthCheck.Frontend\dist\healthcheck.js");
            return new FileStreamResult(new FileStream(filepath, FileMode.Open), "content-disposition");
        }

        //[OutputCache(Duration = 1200, VaryByParam = "none")]
        [RequestLogInfo(hide: true)]
        public FileResult GetVendorScript()
        {
            var filepath = Path.GetFullPath($@"{HostingEnvironment.MapPath("~")}..\HealthCheck.Frontend\dist\healthcheck.vendor.js");
            return new FileStreamResult(new FileStream(filepath, FileMode.Open), "content-disposition");
        }

        public async Task<ActionResult> RunHealthChecks()
        {
            var results = await TestRunner.ExecuteTests(TestDiscoverer, 
                testFilter: (test) => test.Categories.Contains(RuntimeTestConstants.Categories.ScheduledHealthCheck),
                siteEventService: Services.SiteEventService,
                auditEventService: Services.AuditEventService
            );
            return CreateJsonResult(results.Select(x => new { Test = x.Test?.Name, Result = x.Message, SiteEventTitle = x.SiteEvent?.Title }));
        }

        // New mock data
        public async Task<ActionResult> AddEvents(bool reInitService = true)
        {
            if (reInitService)
            {
                Services.SiteEventService = CreateSiteEventService();
            }

            if ((await Services.SiteEventService.GetEvents(DateTime.MinValue, DateTime.MaxValue)).Count == 0)
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

        public ActionResult AddDataflow(int count = 10)
        {
            var entriesToInsert = Enumerable.Range(1, count)
                .Select(i => new TestEntry
                {
                    Code = $"000{i}-P",
                    Name = $"Entry [{DateTime.Now.ToLongTimeString()}]"
                })
                .ToList();

            testStreamA.InsertEntries(entriesToInsert);

            return Content("OK :]");
        }

        // New mock data
        private static readonly Random _rand = new Random();
        public async Task<ActionResult> AddEvent()
        {
            if (!Enabled || Services.SiteEventService == null) return HttpNotFound();

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

            await Services.SiteEventService.StoreEvent(ev);
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
