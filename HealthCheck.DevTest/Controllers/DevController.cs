using HealthCheck.Core.Abstractions;
using HealthCheck.Core.Attributes;
using HealthCheck.Core.Extensions;
using HealthCheck.Core.Modules.AuditLog;
using HealthCheck.Core.Modules.AuditLog.Abstractions;
using HealthCheck.Core.Modules.Dataflow;
using HealthCheck.Core.Modules.Dataflow.Abstractions;
using HealthCheck.Core.Modules.Dataflow.Models;
using HealthCheck.Core.Modules.Dataflow.Services;
using HealthCheck.Core.Modules.Documentation;
using HealthCheck.Core.Modules.Documentation.Services;
using HealthCheck.Core.Modules.EventNotifications;
using HealthCheck.Core.Modules.EventNotifications.Abstractions;
using HealthCheck.Core.Modules.EventNotifications.Notifiers;
using HealthCheck.Core.Modules.EventNotifications.Services;
using HealthCheck.Core.Modules.LogViewer;
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
using HealthCheck.Module.DynamicCodeExecution.Abstractions;
using HealthCheck.Module.DynamicCodeExecution.AutoComplete.MCA;
using HealthCheck.Module.DynamicCodeExecution.Models;
using HealthCheck.Module.DynamicCodeExecution.Module;
using HealthCheck.Module.DynamicCodeExecution.PreProcessors;
using HealthCheck.Module.DynamicCodeExecution.Storage;
using HealthCheck.Module.DynamicCodeExecution.Validators;
using HealthCheck.Modules.DevModule;
using HealthCheck.RequestLog.Services;
using HealthCheck.WebUI.Abstractions;
using HealthCheck.WebUI.Models;
using HealthCheck.WebUI.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Hosting;
using System.Web.Http;
using System.Web.Mvc;

namespace HealthCheck.DevTest.Controllers
{
    public class DevController : HealthCheckControllerBase<RuntimeTestAccessRole>
    {
        #region Props
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
        private IEventDataSink EventSink { get; set; }
        private static bool ForceLogout { get; set; }
        #endregion

        #region Init
        public DevController()
        {
            InitServices();

            UseModule(new HCDynamicCodeExecutionModule(new HCDynamicCodeExecutionModuleOptions() {
                TargetAssembly = typeof(DevController).Assembly,
                ScriptStorage = new FlatFileDynamicCodeScriptStorage(@"C:\temp\DCE_Scripts.json"),
                PreProcessors = new IDynamicCodePreProcessor[]
                {
                    new BasicAutoCreateUsingsPreProcessor(typeof(DevController).Assembly),
                    new WrapUsingsInRegionPreProcessor(),
                    new FuncPreProcessor("'woot' replacer", (p, code) => code.Replace("woot", "w00t"), "Replaces any instances of 'woot' with 'w00t'"),
                    new FuncPreProcessor("Bad words be gone", (p, code) => code.Replace(" bad ", "***"), canBeDisabled: false)
                },
                Validators = new IDynamicCodeValidator[]
                {
                    new FuncCodeValidator((code) =>
                        code.Contains("format c:")
                            ? DynamicCodeValidationResult.Deny("No format pls")
                            : DynamicCodeValidationResult.Allow()
                    )
                },
                AutoCompleter = new DCEMCAAutoCompleter()
            }));
            UseModule(new HCTestsModule(new HCTestsModuleOptions() { AssemblyContainingTests = typeof(DevController).Assembly }))
                .ConfigureGroups((options) => options
                    .ConfigureGroup(RuntimeTestConstants.Group.AdminStuff, uiOrder: 100)
                    .ConfigureGroup(RuntimeTestConstants.Group.AlmostTopGroup, uiOrder: 50)
                    .ConfigureGroup(RuntimeTestConstants.Group.AlmostBottomGroup, uiOrder: -20)
                    .ConfigureGroup(RuntimeTestConstants.Group.BottomGroup, uiOrder: -50)
                );
            UseModule(new HCEventNotificationsModule(new HCEventNotificationsModuleOptions() { EventSink = EventSink }));
            UseModule(new HCLogViewerModule(new HCLogViewerModuleOptions() { LogSearcherService = CreateLogSearcherService() }));
            UseModule(new HCDocumentationModule(new HCDocumentationModuleOptions()
            {
                EnableDiagramDetails = true,
                EnableDiagramSandbox = true,
                SequenceDiagramService = new DefaultSequenceDiagramService(new DefaultSequenceDiagramServiceOptions()
                {
                    DefaultSourceAssemblies = new[] { typeof(DevController).Assembly }
                }),
                FlowChartsService = new DefaultFlowChartService(new DefaultFlowChartServiceOptions()
                {
                    DefaultSourceAssemblies = new[] { typeof(DevController).Assembly }
                })
            }));
            UseModule(new HCDataflowModule<RuntimeTestAccessRole>(new HCDataflowModuleOptions<RuntimeTestAccessRole>() { DataflowService = DataflowService }));
            UseModule(new HCAuditLogModule(new HCAuditLogModuleOptions() { AuditEventService = _auditEventService }));
            UseModule(new HCSiteEventsModule(new HCSiteEventsModuleOptions() { SiteEventService = _siteEventService }));
            UseModule(new HCRequestLogModule(new HCRequestLogModuleOptions() { RequestLogService = RequestLogServiceAccessor.Current }));
            UseModule(new HCSettingsModule(new HCSettingsModuleOptions() { SettingsService = SettingsService }));
            //UseModule(new TestModuleB(), "[tst]");
            //UseModule(new TestModuleA(), "Dev");

            if (!_hasInited)
            {
                InitOnce();
            }
        }
        #endregion

        #region Overrides
        protected override void ConfigureAccess(HttpRequestBase request, AccessConfig<RuntimeTestAccessRole> config)
        {
            /// MODULES //
            config.GiveRolesAccessToModule(
                RuntimeTestAccessRole.Guest | RuntimeTestAccessRole.WebAdmins,
                TestModuleA.TestModuleAAccessOption.DeleteThing | TestModuleA.TestModuleAAccessOption.EditThing
            );

            config.GiveRolesAccessToModule(RuntimeTestAccessRole.SystemAdmins, TestModuleB.TestModuleBAccessOption.NumberOne);

            // HCDynamicCodeExecutionModule
            // Guest => View module. Edit local scripts only
            config.GiveRolesAccessToModule(RuntimeTestAccessRole.Guest, HCDynamicCodeExecutionModule.AccessOption.Nothing);
            // WebAdmins => only execute directly
            config.GiveRolesAccessToModule(RuntimeTestAccessRole.WebAdmins, HCDynamicCodeExecutionModule.AccessOption.ExecuteCustomScript);
            // SomethingElse => only create new ones
            config.GiveRolesAccessToModule(RuntimeTestAccessRole.SomethingElse, HCDynamicCodeExecutionModule.AccessOption.CreateNewScriptOnServer);
            // API => load and execute existing scripts
            config.GiveRolesAccessToModule(RuntimeTestAccessRole.API,
                HCDynamicCodeExecutionModule.AccessOption.ExecuteSavedScript | HCDynamicCodeExecutionModule.AccessOption.LoadScriptFromServer);

            config.GiveRolesAccessToModuleWithFullAccess<HCTestsModule>(RuntimeTestAccessRole.WebAdmins);
            config.GiveRolesAccessToModuleWithFullAccess<HCSettingsModule>(RuntimeTestAccessRole.WebAdmins);
            config.GiveRolesAccessToModuleWithFullAccess<HCRequestLogModule>(RuntimeTestAccessRole.WebAdmins);
            config.GiveRolesAccessToModuleWithFullAccess<HCSiteEventsModule>(RuntimeTestAccessRole.WebAdmins);
            config.GiveRolesAccessToModuleWithFullAccess<HCAuditLogModule>(RuntimeTestAccessRole.WebAdmins);
            config.GiveRolesAccessToModuleWithFullAccess<HCDataflowModule<RuntimeTestAccessRole>>(RuntimeTestAccessRole.WebAdmins);
            config.GiveRolesAccessToModuleWithFullAccess<HCDocumentationModule>(RuntimeTestAccessRole.WebAdmins);
            config.GiveRolesAccessToModuleWithFullAccess<HCLogViewerModule>(RuntimeTestAccessRole.WebAdmins);
            config.GiveRolesAccessToModuleWithFullAccess<HCEventNotificationsModule>(RuntimeTestAccessRole.WebAdmins);
            config.GiveRolesAccessToModuleWithFullAccess<HCDynamicCodeExecutionModule>(RuntimeTestAccessRole.SystemAdmins);
            //////////////

            config.ShowFailedModuleLoadStackTrace = new Maybe<RuntimeTestAccessRole>(RuntimeTestAccessRole.WebAdmins);
            config.PingAccess = new Maybe<RuntimeTestAccessRole>(RuntimeTestAccessRole.API);
            config.RedirectTargetOnNoAccess = "/no-access";
        }

        public override ActionResult Index()
        {
            EventSink.RegisterEvent("pageload", new {
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
                        EventSink.RegisterEvent("event_parallel_test", new
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
                    EventSink.RegisterEvent("thing_imported", new
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

        protected override HCFrontEndOptions GetFrontEndOptions()
            => new HCFrontEndOptions(EndpointBase)
            {
                ApplicationTitle = "Test Monitor",
                ApplicationTitleLink = "/?sysadmin=x&webadmin=1",
                EditorConfig = new HCFrontEndOptions.EditorWorkerConfig
                {
                    EditorWorkerUrl = $"{EndpointBase}/getscript?name=editor.worker.js",
                    JsonWorkerUrl = $"{EndpointBase}/getscript?name=json.worker.js"
                }
            };

        protected override HCPageOptions GetPageOptions()
            => new HCPageOptions()
            {
                PageTitle = "Test Monitor",
                JavaScriptUrls = new List<string> {
                    $"{EndpointBase}/GetVendorScript",
                    $"{EndpointBase}/GetMainScript",
                }
            };

        protected override RequestInformation<RuntimeTestAccessRole> GetRequestInformation(HttpRequestBase request)
        {
            EventSink.RegisterEvent("GetRequestInfo", new
            {
                Type = this.GetType().Name,
                Path = Request?.Path
            });

            if (ForcedRole != null)
            {
                return new RequestInformation<RuntimeTestAccessRole>(ForcedRole.Value, "forcedId", "Forced role user");
            }

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
        [HideFromRequestLog]
        public ActionResult GetMainScript() => LoadFile("healthcheck.js");
        
        //[OutputCache(Duration = 1200, VaryByParam = "none")]
        [HideFromRequestLog]
        public ActionResult GetVendorScript() => LoadFile("healthcheck.vendor.js");

        [HideFromRequestLog]
        public ActionResult GetScript([FromUri]string name) => LoadFile(name);

        private ActionResult LoadFile(string filename)
        {
            var filepath = Path.GetFullPath($@"{HostingEnvironment.MapPath("~")}..\HealthCheck.Frontend\dist\{filename}");
            if (!System.IO.File.Exists(filepath)) return Content("");
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
            EventSink.RegisterEvent("pageload", payload);
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

        private static RuntimeTestAccessRole? ForcedRole { get; set; }
        public ActionResult ForceAccessRole(string name = null)
        {
            if (name == null)
            {
                ForcedRole = null;
                return Content($"Cleared forced role.");
            }
            else
            {
                ForcedRole = (RuntimeTestAccessRole) Enum.Parse(typeof(RuntimeTestAccessRole), name);
                return Content($"Role set to: {ForcedRole}.");
            }
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

        private void InitServices()
        {
            if (_siteEventService == null)
            {
                _siteEventService = CreateSiteEventService();
                _auditEventService = CreateAuditEventService();
            }

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
            EventSink = new DefaultEventDataSink(EventSinkNotificationConfigStorage, EventSinkNotificationDefinitionStorage)
                .AddNotifier(new WebHookEventNotifier())
                .AddNotifier(new MyNotifier())
                .AddNotifier(new SimpleNotifier())
                .AddPlaceholder("NOW", () => DateTime.Now.ToString())
                .AddPlaceholder("ServerName", () => Environment.MachineName);
            (EventSink as DefaultEventDataSink).IsEnabled = () => SettingsService.GetValue<TestSettings, bool>(x => x.EnableEventRegistering);
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
