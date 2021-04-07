using HealthCheck.Core.Abstractions;
using HealthCheck.Core.Attributes;
using HealthCheck.Core.Config;
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
using HealthCheck.Core.Modules.Messages;
using HealthCheck.Core.Modules.Messages.Abstractions;
using HealthCheck.Core.Modules.Messages.Models;
using HealthCheck.Core.Modules.Metrics;
using HealthCheck.Core.Modules.Metrics.Context;
using HealthCheck.Core.Modules.SecureFileDownload;
using HealthCheck.Core.Modules.SecureFileDownload.Abstractions;
using HealthCheck.Core.Modules.SecureFileDownload.FileStorage;
using HealthCheck.Core.Modules.SecureFileDownload.Models;
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
using HealthCheck.Module.DynamicCodeExecution.Abstractions;
using HealthCheck.Module.DynamicCodeExecution.Models;
using HealthCheck.Module.DynamicCodeExecution.Module;
using HealthCheck.Module.DynamicCodeExecution.PreProcessors;
using HealthCheck.Module.DynamicCodeExecution.Storage;
using HealthCheck.Module.DynamicCodeExecution.Validators;
using HealthCheck.Module.EndpointControl.Abstractions;
using HealthCheck.Module.EndpointControl.Module;
using HealthCheck.RequestLog.Services;
using HealthCheck.WebUI.Abstractions;
using HealthCheck.WebUI.Models;
using HealthCheck.WebUI.Services;
using HealthCheck.WebUI.TFA.Util;
using System;
using System.Collections.Generic;
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
        #region Props & Fields
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
        //private static readonly HCMemoryMessageStore _memoryMessageStore = new HCMemoryMessageStore()
        private static readonly IHCMessageStorage _memoryMessageStore = new HCFlatFileMessageStore(@"c:\temp\hc_messages");
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

        #region Init
        public DevController()
        {
            InitServices();

            var assemblies = new[]
            {
                typeof(DevController).Assembly,
                typeof(RuntimeTestConstants).Assembly
            };

            UseModule(new HCMetricsModule(new HCMetricsModuleOptions()));
            UseModule(new HCTestsModule(new HCTestsModuleOptions()
            {
                AssembliesContainingTests = assemblies,
                ReferenceParameterFactories = CreateReferenceParameterFactories
                //JsonInputTemplateFactory = (type) =>
                //{
                //    if (type == typeof(System.Net.Mail.MailMessage))
                //    {
                //        return HCTestsJsonTemplateResult.CreateNoTemplate();
                //    }
                //    return HCTestsJsonTemplateResult.CreateDefault();
                //}
            }))
                .ConfigureGroups((options) => options
                    .ConfigureGroup(RuntimeTestConstants.Group.TopGroup, uiOrder: 110)
                    .ConfigureGroup(RuntimeTestConstants.Group.AdminStuff, uiOrder: 100)
                    .ConfigureGroup(RuntimeTestConstants.Group.AlmostTopGroup, uiOrder: 50)
                    .ConfigureGroup(RuntimeTestConstants.Group.AlmostBottomGroup, uiOrder: -20)
                    .ConfigureGroup(RuntimeTestConstants.Group.BottomGroup, uiOrder: -50)
                );
            UseModule(new HCMessagesModule(new HCMessagesModuleOptions() { MessageStorage = _memoryMessageStore }
                .DefineInbox("mail", "Mail", "All sent email ends up here.")
                .DefineInbox("sms", "SMS", "All sent sms ends up here.")
            ));
            UseModule(new HCEndpointControlModule(new HCEndpointControlModuleOptions()
            {
                EndpointControlService = HCGlobalConfig.GetDefaultInstanceResolver()(typeof(IEndpointControlService)) as IEndpointControlService,
                RuleStorage = HCGlobalConfig.GetDefaultInstanceResolver()(typeof(IEndpointControlRuleStorage)) as IEndpointControlRuleStorage,
                DefinitionStorage = HCGlobalConfig.GetDefaultInstanceResolver()(typeof(IEndpointControlEndpointDefinitionStorage)) as IEndpointControlEndpointDefinitionStorage,
                HistoryStorage = HCGlobalConfig.GetDefaultInstanceResolver()(typeof(IEndpointControlRequestHistoryStorage)) as IEndpointControlRequestHistoryStorage
            }));
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

            UseModule(new HCDynamicCodeExecutionModule(new HCDynamicCodeExecutionModuleOptions() {
                StoreCopyOfExecutedScriptsAsAuditBlobs = true,
                TargetAssembly = typeof(DevController).Assembly,
                ScriptStorage = new FlatFileDynamicCodeScriptStorage(@"C:\temp\DCE_Scripts.json"),
                PreProcessors = new IDynamicCodePreProcessor[]
                {
                    new BasicAutoCreateUsingsPreProcessor(typeof(DevController).Assembly){
                        IncludeReferencedAssemblies = true
                    },
                    new WrapUsingsInRegionPreProcessor(),
                    new FuncPreProcessor("'test' replacer", (p, code) => code.Replace("test", "TEST"), "Replaces any instances of 'test' with 'TEST'"),
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
                AutoCompleter = null,// new DCEMCAAutoCompleter(),
                StaticSnippets = new List<CodeSuggestion>
                {
                    new CodeSuggestion("GetService<T>(id)", "Get a registered service", "GetService<${1:T}>(${2:x})")
                }
            }));
            UseModule(new HCEventNotificationsModule(new HCEventNotificationsModuleOptions() { EventSink = EventSink }));
            UseModule(new HCLogViewerModule(new HCLogViewerModuleOptions() { LogSearcherService = CreateLogSearcherService() }));
            UseModule(new HCDocumentationModule(new HCDocumentationModuleOptions()
            {
                EnableDiagramDetails = true,
                EnableDiagramSandbox = true,
                SequenceDiagramService = new DefaultSequenceDiagramService(new DefaultSequenceDiagramServiceOptions()
                {
                    DefaultSourceAssemblies = assemblies
                }),
                FlowChartsService = new DefaultFlowChartService(new DefaultFlowChartServiceOptions()
                {
                    DefaultSourceAssemblies = assemblies
                })
            }));
            UseModule(new HCDataflowModule<RuntimeTestAccessRole>(new HCDataflowModuleOptions<RuntimeTestAccessRole>() { DataflowService = DataflowService }));
            UseModule(new HCAuditLogModule(new HCAuditLogModuleOptions() { AuditEventService = _auditEventService, IncludeClientConnectionDetailsInAllEvents = true }));
            UseModule(new HCSiteEventsModule(new HCSiteEventsModuleOptions() { SiteEventService = _siteEventService }));
            UseModule(new HCRequestLogModule(new HCRequestLogModuleOptions() { RequestLogService = RequestLogServiceAccessor.Current }));
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
        #endregion

        #region Overrides
        protected override void ConfigureAccess(HttpRequestBase request, AccessConfig<RuntimeTestAccessRole> config)
        {
            // MODULES //
            config.GiveRolesAccessToModule(
                RuntimeTestAccessRole.Guest | RuntimeTestAccessRole.WebAdmins,
                TestModuleA.TestModuleAAccessOption.DeleteThing | TestModuleA.TestModuleAAccessOption.EditThing
            );

            config.GiveRolesAccessToModule(RuntimeTestAccessRole.SystemAdmins, TestModuleB.TestModuleBAccessOption.NumberOne);

            // HCDynamicCodeExecutionModule
            // Guest => View module. Edit local scripts only
            config.GiveRolesAccessToModule(RuntimeTestAccessRole.Guest, HCDynamicCodeExecutionModule.AccessOption.None);
            // WebAdmins => only execute directly
            config.GiveRolesAccessToModule(RuntimeTestAccessRole.WebAdmins, HCDynamicCodeExecutionModule.AccessOption.ExecuteCustomScript);
            // SomethingElse => only create new ones
            config.GiveRolesAccessToModule(RuntimeTestAccessRole.SomethingElse, HCDynamicCodeExecutionModule.AccessOption.CreateNewScriptOnServer);
            // API => load and execute existing scripts
            config.GiveRolesAccessToModule(RuntimeTestAccessRole.API,
                HCDynamicCodeExecutionModule.AccessOption.ExecuteSavedScript | HCDynamicCodeExecutionModule.AccessOption.LoadScriptFromServer);

            config.GiveRolesAccessToModuleWithFullAccess<HCMetricsModule>(RuntimeTestAccessRole.WebAdmins);
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
            config.GiveRolesAccessToModuleWithFullAccess<HCAccessTokensModule>(RuntimeTestAccessRole.SystemAdmins);
            config.GiveRolesAccessToModuleWithFullAccess<HCSecureFileDownloadModule>(RuntimeTestAccessRole.WebAdmins);
            config.GiveRolesAccessToModuleWithFullAccess<HCEndpointControlModule>(RuntimeTestAccessRole.WebAdmins);
            config.GiveRolesAccessToModuleWithFullAccess<HCMessagesModule>(RuntimeTestAccessRole.WebAdmins);
            //////////////

            config.ShowFailedModuleLoadStackTrace = new Maybe<RuntimeTestAccessRole>(RuntimeTestAccessRole.WebAdmins);
            config.PingAccess = new Maybe<RuntimeTestAccessRole>(RuntimeTestAccessRole.API);
            config.RedirectTargetOnNoAccess = "/no-access";
            config.IntegratedLoginConfig = new HCIntegratedLoginConfig
            {
                IntegratedLoginEndpoint = "/hclogin/login",
                Show2FAInput = true,
                Current2FACodeExpirationTime = HealthCheck2FAUtil.GetCurrentCodeExpirationTime(),
                Send2FACodeEndpoint = "/hclogin/Request2FACode"
            };
        }

        public override ActionResult Index()
        {
            EventSink.RegisterEvent("pageload", new {
                Url = Request.RawUrl,
                User = CurrentRequestInformation?.UserName,
                SettingValue = SettingsService.GetSettings<TestSettings>().IntProp
            });

            if (Request.QueryString.AllKeys.Contains("addmessages"))
            {
                for (int i = 0; i < 10; i++)
                {
                    var msg = new HCDefaultMessageItem($"Some summary here #{i}", $"{i}345678", $"841244{i}", $"Some test message #{i} here etc etc.", false);
                    if (i % 4 == 0)
                    {
                        msg.SetError("Failed to send because of server error.");
                    }
                    _memoryMessageStore.StoreMessage("sms", msg);
                }
                for (int i = 0; i < 13; i++)
                {
                    var msg = new HCDefaultMessageItem($"Subject #{i}, totally not spam", $"test_{i}@somewhe.re", $"to@{i}mail.com",
                            $"<html>" +
                            $"<body>" +
                            $"<style>" +
                            $"div {{" +
                            $"display: inline-block;" +
                            $"font-size: 40px !important;" +
                            $"color: red !important;" +
                            $"}}" +
                            $"</style>" +
                            $"<h3>Super fancy contents here!</h3>Now <b>this</b> is a mail! #{i} or <div>something</div> <img src=\"https://picsum.photos/200\" />.'" +
                            $"</body>" +
                            $"</html>", true);
                    if (i % 5 == 0)
                    {
                        msg.SetError("Failed to send because of invalid email.");
                    }
                    _memoryMessageStore.StoreMessage("mail", msg);
                }
            }

            if (FlatFileSecureFileDownloadDefinitionStorage.GetDefinitionByUrlSegmentText("test") == null)
            {
                FlatFileSecureFileDownloadDefinitionStorage.CreateDefinition(new SecureFileDownloadDefinition
                {
                    CreatedAt = DateTimeOffset.Now,
                    //DownloadCountLimit = 5,
                    //ExpiresAt = 
                    FileId = "testA.jpg",
                    FileName = "Test File A.jpg",
                    Id = Guid.NewGuid(),
                    StorageId = "files_test",
                    UrlSegmentText = "test"
                });
            }
            if (FlatFileSecureFileDownloadDefinitionStorage.GetDefinitionByUrlSegmentText("test_url") == null)
            {
                FlatFileSecureFileDownloadDefinitionStorage.CreateDefinition(new SecureFileDownloadDefinition
                {
                    CreatedAt = DateTimeOffset.Now,
                    //DownloadCountLimit = 5,
                    //ExpiresAt = 
                    FileId = "https://via.placeholder.com/500x400",
                    FileName = "Test File B.jpg",
                    Id = Guid.NewGuid(),
                    StorageId = "urls_test",
                    UrlSegmentText = "test_url"
                });
            }

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
                            TimeStamp = DateTimeOffset.Now,
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
                memoryStream.InsertEntry($"Test item @ {DateTimeOffset.Now}");
                testStreamA.InsertEntries(someExternalItems);
            }

            if (Request.QueryString.AllKeys.Contains("events") && int.TryParse(Request.QueryString["events"], out int eventCount))
            {
                for (int i=0;i< eventCount; i++)
                {
                    EventSink.RegisterEvent("thing_imported", new
                    {
                        Code = 9999 + i,
                        DisplayName = $"Some item #{9999 + i}",
                        PublicUrl = $"/products/item_{i}"
                    });
                }
            }

            return base.Index();
        }

        protected override HCFrontEndOptions GetFrontEndOptions()
            => new HCFrontEndOptions(EndpointBase)
            {
                ApplicationTitle = "HealthCheck",
                ApplicationTitleLink = "/?sysadmin=x&webadmin=1",
                EditorConfig = new HCFrontEndOptions.EditorWorkerConfig
                {
                    //EditorWorkerUrl = "blob:https://unpkg.com/christianh-healthcheck@3.0.5/editor.worker.js",
                    //JsonWorkerUrl = "blob:https://unpkg.com/christianh-healthcheck@3.0.5/json.worker.js"
                    EditorWorkerUrl = $"{EndpointBase}/getscript?name=editor.worker.js",
                    JsonWorkerUrl = $"{EndpointBase}/getscript?name=json.worker.js"
                }
            };

        protected override HCPageOptions GetPageOptions()
            => new HCPageOptions()
            {
                PageTitle = "HealthCheck",
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
            var filepath = Path.GetFullPath($@"{HostingEnvironment.MapPath("~")}..\..\HealthCheck.Frontend\dist\{filename}");
            if (!System.IO.File.Exists(filepath)) return Content("");
            return new FileStreamResult(System.IO.File.Open(filepath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite), "content-disposition");
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
                    SettingValue = SettingsService.GetSettings<TestSettings>().IntProp,
                    ExtraB = "BBBB"
                },
                2 => new
                {
                    Url = Request.RawUrl,
                    User = CurrentRequestInformation?.UserName,
                    SettingValue = SettingsService.GetSettings<TestSettings>().IntProp,
                    ExtraA = "AAAA"
                },
                _ => new
                {
                    Url = Request.RawUrl,
                    User = CurrentRequestInformation?.UserName,
                    SettingValue = SettingsService.GetSettings<TestSettings>().IntProp
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

        public async Task<ActionResult> MetricsTest()
        {
            HCMetricsContext.StartTiming("Total");

            HCMetricsContext.StartTiming("First part");
            await Task.Delay(TimeSpan.FromSeconds(0.1));
            HCMetricsContext.EndTiming();

            HCMetricsContext.StartTiming("Second part");
            await Task.Delay(TimeSpan.FromSeconds(0.2));
            HCMetricsContext.EndTiming();

            HCMetricsContext.AddNote("What just happened? 🤔");

            HCMetricsContext.StartTiming("Last part");
            await Task.Delay(TimeSpan.FromSeconds(0.075));
            HCMetricsContext.EndTiming();

            return Content("Ok");
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

        private ILogSearcherService CreateLogSearcherService()
            => new FlatFileLogSearcherService(new FlatFileLogSearcherServiceOptions()
                    .IncludeLogFilesInDirectory(HostingEnvironment.MapPath("~/App_Data/TestLogs/")));

        private ISiteEventService CreateSiteEventService()
            => new SiteEventService(new FlatFileSiteEventStorage(HostingEnvironment.MapPath("~/App_Data/SiteEventStorage.json"),
                maxEventAge: TimeSpan.FromDays(5), delayFirstCleanup: false));

        private IAuditEventStorage CreateAuditEventService()
        {
            var blobFolder = HostingEnvironment.MapPath("~/App_Data/AuditEventBlobs");
            var blobService = new FlatFileAuditBlobStorage(blobFolder, maxEventAge: TimeSpan.FromDays(1));
            return new FlatFileAuditEventStorage(HostingEnvironment.MapPath("~/App_Data/AuditEventStorage.json"),
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
        public async Task<ActionResult> AddEvent()
        {
            if (!Enabled || _siteEventService == null) return HttpNotFound();

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
            ) {
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
            description = $"{subject} {accident} and {reactor} is {reaction}.".CapitalizeFirst();
        }

        private readonly string[] _subjectXFixes = new[] { "the ", "an unknown ", "most of the |s", "several of the |s", "one of the |s" };
        private readonly string[] _subjects = new[] { "service", "server", "integration", "frontpage", "developer", "codebase", "project manager", "CEO" };
        private readonly string[] _accidents = new[] { "is on fire", "exploded", "is slow", "decided to close", "is infected with ransomware", "is not happy", "don't know what to do" };
        private readonly string[] _reactions = new[] { "on fire", "not pleased", "confused", "not happy", "leaving" };
        #endregion
    }
}
