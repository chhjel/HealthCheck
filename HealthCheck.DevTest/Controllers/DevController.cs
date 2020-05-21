using HealthCheck.Core.Abstractions;
using HealthCheck.Core.Attributes;
using HealthCheck.Core.Enums;
using HealthCheck.Core.Extensions;
using HealthCheck.Core.Modules.AuditLog;
using HealthCheck.Core.Modules.AuditLog.Abstractions;
using HealthCheck.Core.Modules.Dataflow;
using HealthCheck.Core.Modules.Dataflow.Abstractions;
using HealthCheck.Core.Modules.Dataflow.Models;
using HealthCheck.Core.Modules.Dataflow.Services;
using HealthCheck.Core.Modules.Diagrams.FlowCharts;
using HealthCheck.Core.Modules.Diagrams.SequenceDiagrams;
using HealthCheck.Core.Modules.EventNotifications;
using HealthCheck.Core.Modules.EventNotifications.Notifiers;
using HealthCheck.Core.Modules.Settings;
using HealthCheck.Core.Modules.Settings.Abstractions;
using HealthCheck.Core.Modules.Settings.Attributes;
using HealthCheck.Core.Modules.SiteEvents;
using HealthCheck.Core.Modules.SiteEvents.Abstractions;
using HealthCheck.Core.Modules.SiteEvents.Enums;
using HealthCheck.Core.Modules.SiteEvents.Models;
using HealthCheck.Core.Modules.SiteEvents.Services;
using HealthCheck.Core.Modules.Tests;
using HealthCheck.Core.Services;
using HealthCheck.Core.Services.Models;
using HealthCheck.Core.Util;
using HealthCheck.DevTest._TestImplementation;
using HealthCheck.DevTest._TestImplementation.Dataflow;
using HealthCheck.DevTest._TestImplementation.EventNotifier;
using HealthCheck.Modules.DevModule;
using HealthCheck.RequestLog.Services;
using HealthCheck.WebUI.Abstractions;
using HealthCheck.WebUI.Models;
using HealthCheck.WebUI.Services;
using HealthCheck.WebUI.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        private static readonly TestMemoryStream memoryStream = new TestMemoryStream("Memory");
        private static readonly TestMemoryStream otherStream1 = new TestMemoryStream(null);
        private static readonly TestMemoryStream otherStream2 = new TestMemoryStream(null);
        private static readonly FlatFileEventSinkNotificationConfigStorage EventSinkNotificationConfigStorage
            = new FlatFileEventSinkNotificationConfigStorage(@"c:\temp\eventconfigs.json");
        private static readonly FlatFileEventSinkKnownEventDefinitionsStorage EventSinkNotificationDefinitionStorage
            = new FlatFileEventSinkKnownEventDefinitionsStorage(@"c:\temp\eventconfig_defs.json");
        private IHealthCheckSettingsService SettingsService { get; set; } = new FlatFileHealthCheckSettingsService<TestSettings>(@"C:\temp\settings.json");
        private IDataflowService<RuntimeTestAccessRole> DataflowService { get; set; }
        private static bool ForceLogout { get; set; }

        #region Init
        public DevController() : base() {
            if (_siteEventService == null)
            {
                _siteEventService = CreateSiteEventService();
                _auditEventService = CreateAuditEventService();
            }

            Services.AuditEventService = _auditEventService;
            Services.LogSearcherService = CreateLogSearcherService();
            Services.SequenceDiagramService = new DefaultSequenceDiagramService(new DefaultSequenceDiagramServiceOptions()
            {
                DefaultSourceAssemblies = new[] { typeof(DevController).Assembly }
            });
            Services.FlowChartsService = new DefaultFlowChartService(new DefaultFlowChartServiceOptions()
            {
                DefaultSourceAssemblies = new[] { typeof(DevController).Assembly }
            });
            DataflowService = new DefaultDataflowService<RuntimeTestAccessRole>(new DefaultDataflowServiceOptions<RuntimeTestAccessRole>()
            {
                Streams = new IDataflowStream<RuntimeTestAccessRole>[]
                {
                    testStreamA,
                    testStreamB,
                    testStreamC,
                    simpleStream,
                    memoryStream,
                    otherStream1,
                    otherStream2
                }
            });
            Services.EventSink = new DefaultEventDataSink(EventSinkNotificationConfigStorage, EventSinkNotificationDefinitionStorage)
                .AddNotifier(new WebHookEventNotifier())
                .AddNotifier(new MyNotifier())
                .AddNotifier(new SimpleNotifier())
                .AddPlaceholder("NOW", () => DateTime.Now.ToString())
                .AddPlaceholder("ServerName", () => Environment.MachineName);
            (Services.EventSink as DefaultEventDataSink).IsEnabled = () => SettingsService.GetValue<TestSettings, bool>(x => x.EnableEventRegistering);

            UseModule(new HCDataflowModule<RuntimeTestAccessRole>(new HCDataflowModuleOptions<RuntimeTestAccessRole>() { DataflowService = DataflowService }));
            UseModule(new HCAuditLogModule(new HCAuditLogModuleOptions() { AuditEventService = _auditEventService }));
            UseModule(new HCSiteEventsModule(new HCSiteEventsModuleOptions() { SiteEventService = _siteEventService }));
            UseModule(new HCRequestLogModule(new HCRequestLogModuleOptions() { RequestLogService = RequestLogServiceAccessor.Current }));
            UseModule(new HCSettingsModule(new HCSettingsModuleOptions() { SettingsService = SettingsService }));
            UseModule(new HCTestsModule(new HCTestsModuleOptions() { AssemblyContainingTests = typeof(DevController).Assembly }))
                .ConfigureGroups((options) => options
                    .ConfigureGroup(RuntimeTestConstants.Group.AdminStuff, uiOrder: 100)
                    .ConfigureGroup(RuntimeTestConstants.Group.AlmostTopGroup, uiOrder: 50)
                    .ConfigureGroup(RuntimeTestConstants.Group.AlmostBottomGroup, uiOrder: -20)
                    .ConfigureGroup(RuntimeTestConstants.Group.BottomGroup, uiOrder: -50)
                );
            UseModule(new TestModuleA(), "Custom A");
            UseModule(new TestModuleB());

            if (!_hasInited)
            {
                InitOnce();
            }
        }
        #endregion

        #region Overrides
        // ToDo:
        // Zero random properties to access
        // Move things into accessoptions
        // Move SetTestSetGroupsOptions into testmodule options

        protected override void ConfigureAccess(HttpRequestBase request, AccessOptions<RuntimeTestAccessRole> options)
        {
            /// MODULES //
            /// ToDo: options.GiveRolesAccess...
            GiveRolesAccessToModule(
                RuntimeTestAccessRole.Guest | RuntimeTestAccessRole.WebAdmins,
                TestModuleA.TestModuleAAccessOption.DeleteThing | TestModuleA.TestModuleAAccessOption.EditThing
            );

            GiveRolesAccessToModule(RuntimeTestAccessRole.SystemAdmins, TestModuleB.TestModuleBAccessOption.NumberOne);

            GiveRolesAccessToModuleWithFullAccess<HCTestsModule>(RuntimeTestAccessRole.WebAdmins);
            GiveRolesAccessToModuleWithFullAccess<HCSettingsModule>(RuntimeTestAccessRole.WebAdmins);
            GiveRolesAccessToModuleWithFullAccess<HCRequestLogModule>(RuntimeTestAccessRole.WebAdmins);
            GiveRolesAccessToModuleWithFullAccess<HCSiteEventsModule>(RuntimeTestAccessRole.WebAdmins);
            GiveRolesAccessToModuleWithFullAccess<HCAuditLogModule>(RuntimeTestAccessRole.WebAdmins);
            GiveRolesAccessToModuleWithFullAccess<HCDataflowModule<RuntimeTestAccessRole>>(RuntimeTestAccessRole.WebAdmins);
            //////////////

            options.OverviewPageAccess = new Maybe<RuntimeTestAccessRole>(RuntimeTestAccessRole.Guest);
            options.DocumentationPageAccess = new Maybe<RuntimeTestAccessRole>(RuntimeTestAccessRole.WebAdmins);
            options.TestsPageAccess = new Maybe<RuntimeTestAccessRole>(RuntimeTestAccessRole.WebAdmins | RuntimeTestAccessRole.API);
            options.AuditLogAccess = new Maybe<RuntimeTestAccessRole>(RuntimeTestAccessRole.SystemAdmins);
            options.LogViewerPageAccess = new Maybe<RuntimeTestAccessRole>(RuntimeTestAccessRole.SystemAdmins);
            options.InvalidTestsAccess = new Maybe<RuntimeTestAccessRole>(RuntimeTestAccessRole.SystemAdmins);
            options.SiteEventDeveloperDetailsAccess = new Maybe<RuntimeTestAccessRole>(RuntimeTestAccessRole.SystemAdmins);
            options.RequestLogPageAccess = new Maybe<RuntimeTestAccessRole>(RuntimeTestAccessRole.SystemAdmins);
            options.ClearRequestLogAccess = new Maybe<RuntimeTestAccessRole>(RuntimeTestAccessRole.SystemAdmins);
            options.PingAccess = new Maybe<RuntimeTestAccessRole>(RuntimeTestAccessRole.API);
            options.DataflowPageAccess = new Maybe<RuntimeTestAccessRole>(RuntimeTestAccessRole.WebAdmins);
            options.SettingsPageAccess = new Maybe<RuntimeTestAccessRole>(RuntimeTestAccessRole.SystemAdmins);
            options.EventNotificationsPageAccess = new Maybe<RuntimeTestAccessRole>(RuntimeTestAccessRole.SystemAdmins);
            options.EditEventDefinitionsAccess = new Maybe<RuntimeTestAccessRole>(RuntimeTestAccessRole.SystemAdmins);

            options.RedirectTargetOnNoAccess = "/no-access";
        }

        public override ActionResult Index()
        {
            Services.EventSink.RegisterEvent("pageload", new {
                Url = Request.RawUrl,
                User = CurrentRequestInformation?.UserName,
                SettingValue = SettingsService.GetValue<TestSettings, int>((setting) => setting.IntProp)
            });

            if (Request.QueryString.AllKeys.Contains("eventsink"))
            {
                var count = 10;
                if (int.TryParse(Request.QueryString["eventsink"], out int eventSinkCount))
                {
                    count = eventSinkCount;
                }

                Task.Run(() =>
                {
                    Parallel.ForEach(Enumerable.Range(1, count), (i) =>
                    {
                        Services.EventSink.RegisterEvent("event_parallel_test", new
                        {
                            Number = i,
                            TimeStamp = DateTime.Now,
                            RandomValue = new Random().Next(1000),
                            Guid = Guid.NewGuid()
                        });
                    });
                });
            }

            if (Request.QueryString.AllKeys.Contains("stream"))
            {
                var someExternalItems = new[]
                {
                    new TestEntry() { Code = "6235235", Name = "Name A" },
                    new TestEntry() { Code = "1234", Name = "Name B" },
                    new TestEntry() { Code = "235235", Name = "Name C" }
                };

                simpleStream.InsertEntries(someExternalItems.Select(x => GenericDataflowStreamObject.Create(x)));
                memoryStream.InsertEntry($"Test item @ {DateTime.Now.ToLongTimeString()}");
                testStreamA.InsertEntries(someExternalItems);
            }

            if (Request.QueryString.AllKeys.Contains("events") && int.TryParse(Request.QueryString["events"], out int eventCount))
            {
                var watch = new Stopwatch();
                watch.Start();
                for (int i=0;i< eventCount; i++)
                {
                    Services.EventSink.RegisterEvent("thing_imported", new
                    {
                        Code = 9999 + i,
                        DisplayName = $"Some item #{(9999 + i)}",
                        PublicUrl = $"/products/item_{i}"
                    });
                }
                var elapsed = watch.ElapsedMilliseconds;
            }

            return base.Index();
        }

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
                    HealthCheckPageType.EventNotifications,
                    HealthCheckPageType.Dataflow,
                    HealthCheckPageType.Documentation,
                    HealthCheckPageType.LogViewer,
                    HealthCheckPageType.AuditLog,
                    HealthCheckPageType.Settings
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

        protected override RequestInformation<RuntimeTestAccessRole> GetRequestInformation(HttpRequestBase request)
        {
            Services.EventSink.RegisterEvent("GetRequestInfo", new
            {
                Type = this.GetType().Name,
                Path = Request?.Path
            });

            var roles = RuntimeTestAccessRole.Guest;
            if (request.QueryString["noaccess"] != null)
            {
                roles = RuntimeTestAccessRole.None;
                return new RequestInformation<RuntimeTestAccessRole>(roles, "no_access_test", "No user");
            }

            if (ForceLogout)
            {
                return new RequestInformation<RuntimeTestAccessRole>(roles, "force_logout_test", "No user");
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

        public ActionResult TestEvent(int v = 1)
        {
            object payload = null;
            payload = v switch
            {
                3 => new
                {
                    Url = Request.RawUrl,
                    User = CurrentRequestInformation?.UserName,
                    SettingValue = SettingsService.GetValue<TestSettings, int>((setting) => setting.IntProp),
                    ExtraB = "BBBB"
                },
                2 => new
                {
                    Url = Request.RawUrl,
                    User = CurrentRequestInformation?.UserName,
                    SettingValue = SettingsService.GetValue<TestSettings, int>((setting) => setting.IntProp),
                    ExtraA = "AAAA"
                },
                _ => new
                {
                    Url = Request.RawUrl,
                    User = CurrentRequestInformation?.UserName,
                    SettingValue = SettingsService.GetValue<TestSettings, int>((setting) => setting.IntProp)
                },
            };
            Services.EventSink.RegisterEvent("pageload", payload);
            return Content($"Registered variant #{v}");
        }
        
        public ActionResult Logout()
        {
            ForceLogout = true;
            return Content("Logged out");
        }

        public ActionResult Login()
        {
            ForceLogout = false;
            return Content("Logged in");
        }

        public class TestSettings
        {
            [HealthCheckSetting(description: "Some description here")]
            public string StringProp { get; set; }
            public bool BoolProp { get; set; } = false;
            public int IntProp { get; set; } = 15523;

            [HealthCheckSetting(GroupName = "Service X")]
            public bool EnableX { get; set; }

            [HealthCheckSetting(GroupName = "Service X")]
            public string ConnectionString { get; set; }

            [HealthCheckSetting(GroupName = "Service X")]
            public int Threads { get; set; } = 2;

            [HealthCheckSetting(GroupName = "Service X")]
            public int NumberOfThings { get; set; } = 321;

            [HealthCheckSetting(GroupName = "Event Notifications")]
            public bool EnableEventRegistering { get; set; }
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
            Task.Run(() => AddEvents());
        }

        // New mock data
        public async Task<ActionResult> AddEvents()
        {
            if ((await _siteEventService.GetEvents(DateTime.MinValue, DateTime.MaxValue)).Count == 0)
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
            if (!Enabled || _siteEventService == null) return HttpNotFound();

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

            await _siteEventService.StoreEvent(ev);
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
