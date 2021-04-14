using HealthCheck.Core.Abstractions;
using HealthCheck.Core.Extensions;
using HealthCheck.Core.Models;
using HealthCheck.Core.Modules.AccessTokens;
using HealthCheck.Core.Modules.AuditLog;
using HealthCheck.Core.Modules.AuditLog.Abstractions;
using HealthCheck.Core.Modules.AuditLog.Services;
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
using HealthCheck.Core.Modules.LogViewer.Services;
using HealthCheck.Core.Modules.SecureFileDownload;
using HealthCheck.Core.Modules.SecureFileDownload.Abstractions;
using HealthCheck.Core.Modules.SecureFileDownload.FileStorage;
using HealthCheck.Core.Modules.Settings;
using HealthCheck.Core.Modules.Settings.Abstractions;
using HealthCheck.Core.Modules.Settings.Services;
using HealthCheck.Core.Modules.SiteEvents;
using HealthCheck.Core.Modules.SiteEvents.Abstractions;
using HealthCheck.Core.Modules.SiteEvents.Enums;
using HealthCheck.Core.Modules.SiteEvents.Models;
using HealthCheck.Core.Modules.SiteEvents.Services;
using HealthCheck.Core.Modules.Tests;
using HealthCheck.Core.Modules.Tests.Models;
using HealthCheck.Core.Util;
using HealthCheck.Dev.Common;
using HealthCheck.Dev.Common.Dataflow;
using HealthCheck.Dev.Common.EventNotifier;
using HealthCheck.Dev.Common.Settings;
using HealthCheck.Dev.Common.Tests;
using HealthCheck.Module.DevModule;
using HealthCheck.WebUI.Abstractions;
using HealthCheck.WebUI.Models;
using HealthCheck.WebUI.Services;
using HealthCheck.WebUI.TFA.Util;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace HealthCheck.DevTest.NetCore_3._1.Controllers
{
    [Route("/")]
    public class DevController : HealthCheckControllerBase<RuntimeTestAccessRole>
    {
        #region Props & Fields
        private readonly IWebHostEnvironment _env;
        private const string EndpointBase = "/";
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
        private static readonly FlatFileSecureFileDownloadDefinitionStorage FlatFileSecureFileDownloadDefinitionStorage
            = new FlatFileSecureFileDownloadDefinitionStorage(@"c:\temp\securefile_defs.json");
        private static HCFlatFileStringDictionaryStorage _settingsStorage = new HCFlatFileStringDictionaryStorage(@"C:\temp\settings.json");
        private IHCSettingsService SettingsService { get; set; } = new HCDefaultSettingsService(_settingsStorage);
        private IDataflowService<RuntimeTestAccessRole> DataflowService { get; set; }
        private IEventDataSink EventSink { get; set; }
        private static bool ForceLogout { get; set; }
        #endregion

        public DevController(IWebHostEnvironment env) : base()
        {
            _env = env;

            InitServices();

            UseModule(new HCSecureFileDownloadModule(new HCSecureFileDownloadModuleOptions()
            {
                DefinitionStorage = FlatFileSecureFileDownloadDefinitionStorage,
                FileStorages = new ISecureFileDownloadFileStorage[]
                {
                    new FolderFileStorage("files_test", "Disk storage", @"C:\temp\fileStorageTest"),
                    new UrlFileStorage("urls_test", "External url")
                }
            }));
            UseModule(new HCAccessTokensModule(new HCAccessTokensModuleOptions()
            {
                TokenStorage = new FlatFileAccessManagerTokenStorage(@"C:\temp\AccessTokens.json")
            }));
            UseModule(new HCTestsModule(new HCTestsModuleOptions()
            {
                AssembliesContainingTests = new[]
                    {
                        typeof(DevController).Assembly,
                        typeof(RuntimeTestConstants).Assembly
                    },
                ReferenceParameterFactories = CreateReferenceParameterFactories
            }))
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
            UseModule(new HCSettingsModule(new HCSettingsModuleOptions() { Service = SettingsService, ModelType = typeof(TestSettings) }));

            if (!_hasInited)
            {
                InitOnce();
            }
        }

        private List<RuntimeTestReferenceParameterFactory> CreateReferenceParameterFactories()
        {
            var getUserChoices = new Func<IEnumerable<CustomReferenceType>>(() =>
                Enumerable.Range(1, 50).Select(x => new CustomReferenceType { Id = x, Title = $"Item #{x}" })
            );

            return new List<RuntimeTestReferenceParameterFactory>()
            {
                new RuntimeTestReferenceParameterFactory(
                    typeof(CustomReferenceType),
                    (filter) => getUserChoices()
                        .Where(x => string.IsNullOrWhiteSpace(filter) || x.Title.Contains(filter) || x.Id.ToString().Contains(filter))
                        .Select(x => new RuntimeTestReferenceParameterChoice(x.Id.ToString(), x.Title)),
                    (id) => getUserChoices().FirstOrDefault(x => x.Id.ToString() == id)
                )
            };
        }
        #region Overrides
        protected override HCFrontEndOptions GetFrontEndOptions()
            => new HCFrontEndOptions(EndpointBase)
            {
                ApplicationTitle = "HealthCheck",
                ApplicationTitleLink = "/?sysadmin=x&webadmin=1",
                EditorConfig = new HCFrontEndOptions.EditorWorkerConfig
                {
                    //EditorWorkerUrl = "blob:https://unpkg.com/christianh-healthcheck@3.0.5/editor.worker.js",
                    //JsonWorkerUrl = "blob:https://unpkg.com/christianh-healthcheck@3.0.5/json.worker.js"
                    EditorWorkerUrl = $"{EndpointBase.TrimEnd('/')}/getscript?name=editor.worker.js",
                    JsonWorkerUrl = $"{EndpointBase.TrimEnd('/')}/getscript?name=json.worker.js"
                }
            };

        protected override HCPageOptions GetPageOptions()
            => new HCPageOptions()
            {
                PageTitle = "HealthCheck",
                JavaScriptUrls = new List<string> {
                    $"{EndpointBase.TrimEnd('/')}/GetVendorScript",
                    $"{EndpointBase.TrimEnd('/')}/GetMainScript",
                }
            };

        protected override void ConfigureAccess(HttpRequest request, AccessConfig<RuntimeTestAccessRole> config)
        {
            /// MODULES //
            config.GiveRolesAccessToModule(
                RuntimeTestAccessRole.Guest | RuntimeTestAccessRole.WebAdmins,
                TestModuleA.TestModuleAAccessOption.DeleteThing | TestModuleA.TestModuleAAccessOption.EditThing
            );

            config.GiveRolesAccessToModule(RuntimeTestAccessRole.SystemAdmins, TestModuleB.TestModuleBAccessOption.NumberOne);

            config.GiveRolesAccessToModuleWithFullAccess<HCTestsModule>(RuntimeTestAccessRole.WebAdmins);
            config.GiveRolesAccessToModuleWithFullAccess<HCSettingsModule>(RuntimeTestAccessRole.WebAdmins);
            config.GiveRolesAccessToModuleWithFullAccess<HCSiteEventsModule>(RuntimeTestAccessRole.WebAdmins);
            config.GiveRolesAccessToModuleWithFullAccess<HCAuditLogModule>(RuntimeTestAccessRole.WebAdmins);
            config.GiveRolesAccessToModuleWithFullAccess<HCDataflowModule<RuntimeTestAccessRole>>(RuntimeTestAccessRole.WebAdmins);
            config.GiveRolesAccessToModuleWithFullAccess<HCDocumentationModule>(RuntimeTestAccessRole.WebAdmins);
            config.GiveRolesAccessToModuleWithFullAccess<HCLogViewerModule>(RuntimeTestAccessRole.WebAdmins);
            config.GiveRolesAccessToModuleWithFullAccess<HCEventNotificationsModule>(RuntimeTestAccessRole.WebAdmins);
            config.GiveRolesAccessToModuleWithFullAccess<HCAccessTokensModule>(RuntimeTestAccessRole.SystemAdmins);
            config.GiveRolesAccessToModuleWithFullAccess<HCSecureFileDownloadModule>(RuntimeTestAccessRole.WebAdmins);
            config.GiveRolesAccessToModuleWithFullAccess<TestModuleB>(RuntimeTestAccessRole.WebAdmins);
            //////////////

            config.ShowFailedModuleLoadStackTrace = new Maybe<RuntimeTestAccessRole>(RuntimeTestAccessRole.WebAdmins);
            config.PingAccess = new Maybe<RuntimeTestAccessRole>(RuntimeTestAccessRole.API);
            config.RedirectTargetOnNoAccess = "/no-access";
            config.IntegratedLoginConfig = new HCIntegratedLoginConfig
            {
                IntegratedLoginEndpoint = "/HCLogin/login",
                Show2FAInput = true,
                Current2FACodeExpirationTime = HealthCheck2FAUtil.GetCurrentCodeExpirationTime(),
            };
        }

        protected override RequestInformation<RuntimeTestAccessRole> GetRequestInformation(HttpRequest request)
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
            
            if (request.Query.ContainsKey("noaccess"))
            {
                roles = RuntimeTestAccessRole.None;
                return new RequestInformation<RuntimeTestAccessRole>(roles, "no_access_test", "No user");
            }

            if (ForceLogout)
            {
                return new RequestInformation<RuntimeTestAccessRole>(roles, "force_logout_test", "No user");
            }

            roles |= RuntimeTestAccessRole.SystemAdmins | RuntimeTestAccessRole.WebAdmins;
            if (request.Query.ContainsKey("notsysadmin"))
            {
                roles &= ~RuntimeTestAccessRole.SystemAdmins;
            }
            
            if (request.Query.ContainsKey("key") && request.Query["key"] == "test")
            {
                roles |= RuntimeTestAccessRole.API;
                return new RequestInformation<RuntimeTestAccessRole>(roles, "apitest", "API User");
            }

            return new RequestInformation<RuntimeTestAccessRole>(roles, "dev42core", "Dev core user");
        }
        #endregion

        #region dev
        [Route("GetMainScript")]
        public ActionResult GetMainScript() => LoadFile("healthcheck.js");

        [Route("GetVendorScript")]
        public ActionResult GetVendorScript() => LoadFile("healthcheck.vendor.js");

        [Route("GetScript")]
        public ActionResult GetScript(string name) => LoadFile(name);

        private ActionResult LoadFile(string filename)
        {
            var filepath = GetFilePath($@"..\..\HealthCheck.Frontend\dist\{filename}");
            if (!System.IO.File.Exists(filepath)) return Content("");
            return new FileStreamResult(new FileStream(filepath, FileMode.Open), new MediaTypeHeaderValue("text/plain"))
            {
                FileDownloadName = Path.GetFileName(filepath)
            };
        }

        private string GetFilePath(string relativePath) => Path.GetFullPath(Path.Combine(_env.ContentRootPath, relativePath));

        [Route("TestEvent")]
        public ActionResult TestEvent(int v = 1)
        {
            object payload = null;
            payload = v switch
            {
                3 => new
                {
                    Url = Request.GetDisplayUrl(),
                    User = CurrentRequestInformation?.UserName,
                    SettingValue = SettingsService.GetSettings<TestSettings>().IntProp,
                    ExtraB = "BBBB"
                },
                2 => new
                {
                    Url = Request.GetDisplayUrl(),
                    User = CurrentRequestInformation?.UserName,
                    SettingValue = SettingsService.GetSettings<TestSettings>().IntProp,
                    ExtraA = "AAAA"
                },
                _ => new
                {
                    Url = Request.GetDisplayUrl(),
                    User = CurrentRequestInformation?.UserName,
                    SettingValue = SettingsService.GetSettings<TestSettings>().IntProp
                },
            };
            EventSink.RegisterEvent("pageload", payload);
            return Content($"Registered variant #{v}");
        }

        [Route("Logout")]
        public ActionResult Logout()
        {
            ForceLogout = true;
            return Content("Logged out");
        }

        [Route("Login")]
        public ActionResult Login()
        {
            ForceLogout = false;
            return Content("Logged in");
        }

        private static RuntimeTestAccessRole? ForcedRole { get; set; }

        [Route("ForceAccessRole")]
        public ActionResult ForceAccessRole(string name = null)
        {
            if (name == null)
            {
                ForcedRole = null;
                return Content($"Cleared forced role.");
            }
            else
            {
                ForcedRole = (RuntimeTestAccessRole)Enum.Parse(typeof(RuntimeTestAccessRole), name);
                return Content($"Role set to: {ForcedRole}.");
            }
        }

        private ILogSearcherService CreateLogSearcherService()
            => new FlatFileLogSearcherService(new FlatFileLogSearcherServiceOptions()
                    .IncludeLogFilesInDirectory(GetFilePath(@"App_Data\TestLogs")));

        private ISiteEventService CreateSiteEventService()
            => new SiteEventService(new FlatFileSiteEventStorage(GetFilePath(@"App_Data\SiteEventStorage.json"),
                maxEventAge: TimeSpan.FromDays(5), delayFirstCleanup: false));

        private IAuditEventStorage CreateAuditEventService()
        {
            var blobFolder = GetFilePath(@"App_Data\AuditEventBlobs");
            var blobService = new FlatFileAuditBlobStorage(blobFolder, maxEventAge: TimeSpan.FromDays(1));
            return new FlatFileAuditEventStorage(GetFilePath(@"App_Data\AuditEventStorage.json"),
                maxEventAge: TimeSpan.FromDays(30), delayFirstCleanup: false, blobStorage: blobService);
        }

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
                .AddNotifier(new HCWebHookEventNotifier())
                .AddNotifier(new MyNotifier())
                .AddNotifier(new SimpleNotifier())
                .AddPlaceholder("NOW", () => DateTimeOffset.Now.ToString())
                .AddPlaceholder("ServerName", () => Environment.MachineName);
            (EventSink as DefaultEventDataSink).IsEnabled = () => SettingsService.GetSettings<TestSettings>().EnableEventRegistering;
        }

        // New mock data
        [Route("AddEvents")]
        public async Task<ActionResult> AddEvents()
        {
            if ((await _siteEventService.GetEvents(DateTimeOffset.MinValue, DateTimeOffset.MaxValue)).Count == 0)
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

        [Route("AddDataflow")]
        public ActionResult AddDataflow(int count = 10)
        {
            var entriesToInsert = Enumerable.Range(1, count)
                .Select(i => new TestEntry
                {
                    Code = $"000{i}-P",
                    Name = $"Entry [{DateTimeOffset.Now}]"
                })
                .ToList();

            testStreamA.InsertEntries(entriesToInsert);

            return Content("OK :]");
        }

        // New mock data
        private static readonly Random _rand = new Random();
        [Route("AddEvent")]
        public async Task<ActionResult> AddEvent()
        {
            if (!Enabled || _siteEventService == null) return NotFound();

            CreateSomeData(out string title, out string description);
            var severity = SiteEventSeverity.Information;
            if (_rand.Next(100) < 10)
            {
                severity = SiteEventSeverity.Fatal;
            }
            else if (_rand.Next(100) < 25)
            {
                severity = SiteEventSeverity.Error;
            }
            else if (_rand.Next(100) < 50)
            {
                severity = SiteEventSeverity.Warning;
            }

            var ev = new SiteEvent(
                severity, $"Error type {_rand.Next(10000)}",
                title, description,
                duration: _rand.Next(1, 90)
            )
            {
                Timestamp = DateTimeOffset.Now
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
            }
            else
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
            description = $"{subject} {accident} and {reactor} is {reaction}.".CapitalizeFirst();
        }

        private readonly string[] _subjectXFixes = new[] { "the ", "an unknown ", "most of the |s", "several of the |s", "one of the |s" };
        private readonly string[] _subjects = new[] { "service", "server", "integration", "frontpage", "developer", "codebase", "project manager", "CEO" };
        private readonly string[] _accidents = new[] { "is on fire", "exploded", "is slow", "decided to close", "is infected with ransomware", "is not happy", "don't know what to do" };
        private readonly string[] _reactions = new[] { "on fire", "not pleased", "confused", "not happy", "leaving" };
        #endregion
    }
}
