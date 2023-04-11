using QoDL.Toolkit.Core.Abstractions;
using QoDL.Toolkit.Core.Attributes;
using QoDL.Toolkit.Core.Config;
using QoDL.Toolkit.Core.Models;
using QoDL.Toolkit.Core.Modules.AccessTokens;
using QoDL.Toolkit.Core.Modules.AuditLog;
using QoDL.Toolkit.Core.Modules.AuditLog.Abstractions;
using QoDL.Toolkit.Core.Modules.AuditLog.Services;
using QoDL.Toolkit.Core.Modules.Dataflow;
using QoDL.Toolkit.Core.Modules.Dataflow.Abstractions;
using QoDL.Toolkit.Core.Modules.Dataflow.Models;
using QoDL.Toolkit.Core.Modules.Dataflow.Services;
using QoDL.Toolkit.Core.Modules.Documentation;
using QoDL.Toolkit.Core.Modules.Documentation.Services;
using QoDL.Toolkit.Core.Modules.EventNotifications;
using QoDL.Toolkit.Core.Modules.EventNotifications.Abstractions;
using QoDL.Toolkit.Core.Modules.EventNotifications.Notifiers;
using QoDL.Toolkit.Core.Modules.EventNotifications.Services;
using QoDL.Toolkit.Core.Modules.LogViewer;
using QoDL.Toolkit.Core.Modules.LogViewer.Services;
using QoDL.Toolkit.Core.Modules.Messages;
using QoDL.Toolkit.Core.Modules.Messages.Abstractions;
using QoDL.Toolkit.Core.Modules.Metrics;
using QoDL.Toolkit.Core.Modules.Metrics.Abstractions;
using QoDL.Toolkit.Core.Modules.Metrics.Context;
using QoDL.Toolkit.Core.Modules.ReleaseNotes;
using QoDL.Toolkit.Core.Modules.ReleaseNotes.Abstractions;
using QoDL.Toolkit.Core.Modules.SecureFileDownload;
using QoDL.Toolkit.Core.Modules.SecureFileDownload.Abstractions;
using QoDL.Toolkit.Core.Modules.SecureFileDownload.FileStorage;
using QoDL.Toolkit.Core.Modules.Settings;
using QoDL.Toolkit.Core.Modules.Settings.Abstractions;
using QoDL.Toolkit.Core.Modules.Settings.Services;
using QoDL.Toolkit.Core.Modules.SiteEvents;
using QoDL.Toolkit.Core.Modules.SiteEvents.Abstractions;
using QoDL.Toolkit.Core.Modules.SiteEvents.Services;
using QoDL.Toolkit.Core.Modules.Tests;
using QoDL.Toolkit.Core.Modules.Tests.Models;
using QoDL.Toolkit.Core.Util;
using QoDL.Toolkit.Core.Util.Modules;
using QoDL.Toolkit.Dev.Common;
using QoDL.Toolkit.Dev.Common.Dataflow;
using QoDL.Toolkit.Dev.Common.EventNotifier;
using QoDL.Toolkit.Dev.Common.Metrics;
using QoDL.Toolkit.Dev.Common.Settings;
using QoDL.Toolkit.Dev.Common.Tests;
using QoDL.Toolkit.Dev.Common.Tests.Modules;
using QoDL.Toolkit.DevTest._TestImplementation.Modules;
using QoDL.Toolkit.Module.DataExport;
using QoDL.Toolkit.Module.DataExport.Abstractions;
using QoDL.Toolkit.Module.DataExport.Exporter.Excel;
using QoDL.Toolkit.Module.DevModule;
using QoDL.Toolkit.Module.DynamicCodeExecution.Abstractions;
using QoDL.Toolkit.Module.DynamicCodeExecution.Models;
using QoDL.Toolkit.Module.DynamicCodeExecution.Module;
using QoDL.Toolkit.Module.DynamicCodeExecution.PreProcessors;
using QoDL.Toolkit.Module.DynamicCodeExecution.Storage;
using QoDL.Toolkit.Module.DynamicCodeExecution.Validators;
using QoDL.Toolkit.Module.EndpointControl.Abstractions;
using QoDL.Toolkit.Module.EndpointControl.Module;
using QoDL.Toolkit.RequestLog.Services;
using QoDL.Toolkit.WebUI.Abstractions;
using QoDL.Toolkit.WebUI.MFA.TOTP;
using QoDL.Toolkit.WebUI.Models;
using QoDL.Toolkit.WebUI.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Hosting;
using System.Web.Http;
using System.Web.Mvc;

namespace QoDL.Toolkit.DevTest.Controllers
{
    public class DevController : ToolkitControllerBase<RuntimeTestAccessRole>
    {
        #region Props & Fields
        private const string EndpointBase = "/dev";
        private static ISiteEventService _siteEventService;
        private static IAuditEventStorage _auditEventService;
        public static TestStreamA TestStreamA => DataflowTests.TestStreamA;
        private static readonly TestStreamB testStreamB = new();
        private static readonly TestStreamC testStreamC = new();
        private static readonly SimpleStream simpleStream = new("Simple A");
        private static readonly TestMemoryStream memoryStream = new("Memory");
        private static readonly TestMemoryStream otherStream1 = new(null);
        private static readonly TestMemoryStream otherStream2 = new(null);
        //private static readonly TKMemoryMessageStore _memoryMessageStore = new TKMemoryMessageStore()
        private static readonly ITKMessageStorage _memoryMessageStore = new TKFlatFileMessageStore(@"c:\temp\tk_messages");
        private static readonly FlatFileEventSinkNotificationConfigStorage EventSinkNotificationConfigStorage
            = new(@"c:\temp\eventconfigs.json");
        private static readonly FlatFileEventSinkKnownEventDefinitionsStorage EventSinkNotificationDefinitionStorage
            = new(@"c:\temp\eventconfig_defs.json");
        private static readonly FlatFileSecureFileDownloadDefinitionStorage FlatFileSecureFileDownloadDefinitionStorage
            = new(@"c:\temp\securefile_defs.json");
        private static readonly TKFlatFileStringDictionaryStorage _settingsStorage = new(@"C:\temp\settings.json");
        private ITKSettingsService SettingsService { get; set; } = new TKDefaultSettingsService(_settingsStorage);
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

            UseModule(new TKDataExportModule(new TKDataExportModuleOptions
            {
                Service = TKIoCUtils.GetInstance<ITKDataExportService>(),
                PresetStorage = TKIoCUtils.GetInstance<ITKDataExportPresetStorage>()
            }
                .AddExporter(new TKDataExportExporterXlsx())
            ));
            UseModule(new TKTestsModuleExt(new TKTestsModuleOptions()
            {
                AssembliesContainingTests = assemblies,
                ReferenceParameterFactories = CreateReferenceParameterFactories,
                FileDownloadHandler = (type, id) =>
                {
                    if (id == "404") return null;
                    else if (Guid.TryParse(id, out var fileGuid)) return ToolkitFileDownloadResult.CreateFromString("guid.txt", $"The guid was {id}");
                    else if (id == "ascii") return ToolkitFileDownloadResult.CreateFromString("Success.txt", $"Type: {type}, Id: {id}. ÆØÅæøå etc ôasd. ASCII", encoding: Encoding.ASCII);
                    else return ToolkitFileDownloadResult.CreateFromString("Success.txt", $"Type: {type}, Id: {id}. ÆØÅæøå etc ôasd.");
                },
                DefaultTestAccessLevel = RuntimeTestAccessRole.WebAdmins
                //JsonInputTemplateFactory = (type) =>
                //{
                //    if (type == typeof(System.Net.Mail.MailMessage))
                //    {
                //        return TKTestsJsonTemplateResult.CreateNoTemplate();
                //    }
                //    return TKTestsJsonTemplateResult.CreateDefault();
                //},
            }))
                .ConfigureGroups((options) => options
                    .ConfigureGroup(RuntimeTestConstants.Group.TopGroup, uiOrder: 110)
                    .ConfigureGroup(RuntimeTestConstants.Group.AdminStuff, uiOrder: 100)
                    .ConfigureGroup(RuntimeTestConstants.Group.AlmostTopGroup, uiOrder: 50)
                    .ConfigureGroup(RuntimeTestConstants.Group.AlmostBottomGroup, uiOrder: -20)
                    .ConfigureGroup(RuntimeTestConstants.Group.BottomGroup, uiOrder: -50)
                );
            UseModule(new TKMetricsModule(new TKMetricsModuleOptions()
            {
                Storage = TKIoCUtils.GetInstance<ITKMetricsStorage>()
            }));
            UseModule(new TKReleaseNotesModule(new TKReleaseNotesModuleOptions {
                ReleaseNotesProvider = TKIoCUtils.GetInstance<ITKReleaseNotesProvider>()
            }));
            UseModule(new TKMessagesModule(new TKMessagesModuleOptions() { MessageStorage = _memoryMessageStore }
                .DefineInbox("mail", "Mail", "All sent email ends up here.")
                .DefineInbox("sms", "SMS", "All sent sms ends up here.")
            ));
            UseModule(new TKEndpointControlModule(new TKEndpointControlModuleOptions()
            {
                EndpointControlService = TKGlobalConfig.GetDefaultInstanceResolver()(typeof(IEndpointControlService)) as IEndpointControlService,
                RuleStorage = TKGlobalConfig.GetDefaultInstanceResolver()(typeof(IEndpointControlRuleStorage)) as IEndpointControlRuleStorage,
                DefinitionStorage = TKGlobalConfig.GetDefaultInstanceResolver()(typeof(IEndpointControlEndpointDefinitionStorage)) as IEndpointControlEndpointDefinitionStorage,
                HistoryStorage = TKGlobalConfig.GetDefaultInstanceResolver()(typeof(IEndpointControlRequestHistoryStorage)) as IEndpointControlRequestHistoryStorage
            }));
            UseModule(new TKSecureFileDownloadModule(new TKSecureFileDownloadModuleOptions()
            {
                DefinitionStorage = FlatFileSecureFileDownloadDefinitionStorage,
                FileStorages = new ISecureFileDownloadFileStorage[]
                {
                    new FolderFileStorage("files_testU", "Disk storage (upload only)", @"C:\temp\fileStorageTest") { SupportsSelectingFile = false, SupportsUpload = true },
                    new FolderFileStorage("files_testD", "Disk storage (download only)", @"C:\temp\fileStorageTest") { SupportsSelectingFile = true, SupportsUpload = false },
                    new FolderFileStorage("files_testUD", "Disk storage (upload and download)", @"C:\temp\fileStorageTest") { SupportsSelectingFile = true, SupportsUpload = true },
                    new UrlFileStorage("urls_test", "External url")
                }
            }));
            UseModule(new TKAccessTokensModule(new TKAccessTokensModuleOptions()
            {
                TokenStorage = new FlatFileAccessManagerTokenStorage(@"C:\temp\AccessTokens.json")
            }));

            UseModule(new TKDynamicCodeExecutionModule(new TKDynamicCodeExecutionModuleOptions() {
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
            UseModule(new TKEventNotificationsModule(new TKEventNotificationsModuleOptions() { EventSink = EventSink }));
            UseModule(new TKLogViewerModule(new TKLogViewerModuleOptions() { LogSearcherService = CreateLogSearcherService() }));
            UseModule(new TKDocumentationModule(new TKDocumentationModuleOptions()
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
            UseModule(new TKDataflowModule<RuntimeTestAccessRole>(new TKDataflowModuleOptions<RuntimeTestAccessRole>() { DataflowService = DataflowService }));
            UseModule(new TKAuditLogModule(new TKAuditLogModuleOptions() { AuditEventService = _auditEventService, IncludeClientConnectionDetailsInAllEvents = true }));
            UseModule(new TKSiteEventsModule(new TKSiteEventsModuleOptions() { SiteEventService = _siteEventService }));
            UseModule(new TKRequestLogModule(new TKRequestLogModuleOptions() { RequestLogService = RequestLogServiceAccessor.Current }));
            UseModule(new TKSettingsModule(new TKSettingsModuleOptions() { Service = SettingsService, ModelType = typeof(TestSettings) }));
            UseModule(new TestModuleA());
        }

        private List<RuntimeTestReferenceParameterFactory> CreateReferenceParameterFactories()
        {
            var getUserChoices = new Func<IEnumerable<CustomReferenceType>>(() =>
                Enumerable.Range(1, 50).Select(x => new CustomReferenceType { Id = x, Title = (x % 2 == 0 ? "Item" : "ItemWithSomeLongerValueHereEtcEtcEtc") + $" #{x}" })
            );

            return new List<RuntimeTestReferenceParameterFactory>()
            {
                new RuntimeTestReferenceParameterFactory(
                    typeof(CustomReferenceType),
                    (filter) => getUserChoices()
                        .Where(x => string.IsNullOrWhiteSpace(filter) || x.Title.Contains(filter) || x.Id.ToString().Contains(filter))
                        .Select(x => new RuntimeTestReferenceParameterChoice(x.Id.ToString(), x.Title, $"Id: {x.Id}")),
                    (id) => getUserChoices().FirstOrDefault(x => x.Id.ToString() == id),
                    title: "Custom title here",
                    description: "Custom description here",
                    searchButtonText: "Go!"
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

            // TKDynamicCodeExecutionModule
            // Guest => View module. Edit local scripts only
            config.GiveRolesAccessToModule(RuntimeTestAccessRole.Guest, TKDynamicCodeExecutionModule.AccessOption.None);
            // WebAdmins => only execute directly
            config.GiveRolesAccessToModule(RuntimeTestAccessRole.WebAdmins, TKDynamicCodeExecutionModule.AccessOption.ExecuteCustomScript);
            // SomethingElse => only create new ones
            config.GiveRolesAccessToModule(RuntimeTestAccessRole.SomethingElse, TKDynamicCodeExecutionModule.AccessOption.CreateNewScriptOnServer);
            // API => load and execute existing scripts
            config.GiveRolesAccessToModule(RuntimeTestAccessRole.API,
                TKDynamicCodeExecutionModule.AccessOption.ExecuteSavedScript | TKDynamicCodeExecutionModule.AccessOption.LoadScriptFromServer);

            config.GiveRolesAccessToModuleWithFullAccess<TKMetricsModule>(RuntimeTestAccessRole.WebAdmins);
            var category = request?.QueryString?["testCategory"];
            if (!string.IsNullOrWhiteSpace(category))
            {
                config.GiveRolesAccessToModule(RuntimeTestAccessRole.WebAdmins, TKTestsModule.AccessOption.ViewInvalidTests, new[] { category });
            }
            else
            {
                config.GiveRolesAccessToModuleWithFullAccess<TKTestsModule>(RuntimeTestAccessRole.WebAdmins);
            }
            
            config.GiveRolesAccessToModuleWithFullAccess<TKDataExportModule>(RuntimeTestAccessRole.WebAdmins);
            config.GiveRolesAccessToModuleWithFullAccess<TKSettingsModule>(RuntimeTestAccessRole.WebAdmins);
            config.GiveRolesAccessToModuleWithFullAccess<TKRequestLogModule>(RuntimeTestAccessRole.WebAdmins);
            config.GiveRolesAccessToModuleWithFullAccess<TKSiteEventsModule>(RuntimeTestAccessRole.WebAdmins);
            config.GiveRolesAccessToModuleWithFullAccess<TKAuditLogModule>(RuntimeTestAccessRole.WebAdmins);
            config.GiveRolesAccessToModuleWithFullAccess<TKDataflowModule<RuntimeTestAccessRole>>(RuntimeTestAccessRole.WebAdmins);
            config.GiveRolesAccessToModuleWithFullAccess<TKDocumentationModule>(RuntimeTestAccessRole.WebAdmins);
            config.GiveRolesAccessToModuleWithFullAccess<TKLogViewerModule>(RuntimeTestAccessRole.WebAdmins);
            config.GiveRolesAccessToModuleWithFullAccess<TKEventNotificationsModule>(RuntimeTestAccessRole.WebAdmins);
            config.GiveRolesAccessToModuleWithFullAccess<TKDynamicCodeExecutionModule>(RuntimeTestAccessRole.SystemAdmins);
            config.GiveRolesAccessToModuleWithFullAccess<TKAccessTokensModule>(RuntimeTestAccessRole.SystemAdmins);
            config.GiveRolesAccessToModuleWithFullAccess<TKSecureFileDownloadModule>(RuntimeTestAccessRole.WebAdmins);
            config.GiveRolesAccessToModuleWithFullAccess<TKEndpointControlModule>(RuntimeTestAccessRole.WebAdmins);
            config.GiveRolesAccessToModuleWithFullAccess<TKMessagesModule>(RuntimeTestAccessRole.WebAdmins);
            config.GiveRolesAccessToModuleWithFullAccess<TestModuleA>(RuntimeTestAccessRole.WebAdmins);
            config.GiveRolesAccessToModuleWithFullAccess<TKReleaseNotesModule>(RuntimeTestAccessRole.WebAdmins);
            //config.GiveRolesAccessToModule<TKReleaseNotesModule>(RuntimeTestAccessRole.WebAdmins, new string[] { "Testing" });
            //////////////

            config.ShowFailedModuleLoadStackTrace = new Maybe<RuntimeTestAccessRole>(RuntimeTestAccessRole.WebAdmins);
            config.PingAccess = new Maybe<RuntimeTestAccessRole>(RuntimeTestAccessRole.API);
            //config.RedirectTargetOnNoAccess = "/no-access";
            config.RedirectTargetOnNoAccessUsingRequest = (r, q) => $"/DummyLoginRedirect?r={HttpUtility.UrlEncode($"/?{q}")}";
            config.IntegratedLoginConfig = new TKIntegratedLoginConfig("/TKLogin/login")
                //.EnableOneTimePasswordWithCodeRequest("/tklogin/Request2FACode", "Code pls", required: false)
                .EnableTOTP(required: false)
                .EnableWebAuthn(required: false);

            config.IntegratedProfileConfig = new TKIntegratedProfileConfig
            {
                Username = CurrentRequestInformation.UserName,
                BodyHtml = "Here is some custom content.<ul><li><a href=\"https://www.google.com\">A link here</a></li></ul>",
                // TOTP: Elevate
                ShowTotpElevation = true,
                TotpElevationLogic = async (c) =>
                {
                    await Task.Delay(100);
                    if (!TKMfaTotpUtil.ValidateTotpCode(TKLoginController.DummySecret, c))
                    {
                        return TKGenericResult<TKResultPageAction>.CreateError("Invalid code");
                    }

                    return TKGenericResult<TKResultPageAction>.CreateSuccess(TKResultPageAction.CreateRefresh());
                },
                // TOTP: Add
                ShowAddTotp = true,
                AddTotpLogic = async (pwd, secret, code) =>
                {
                    await Task.Delay(100);
                    if (pwd != "toor") return TKGenericResult<object>.CreateError("Invalid password");
                    if (!TKMfaTotpUtil.ValidateTotpCode(secret, code))
                    {
                        return TKGenericResult.CreateError("Invalid code");
                    }
                    return TKGenericResult.CreateSuccess();
                },
                // TOTP: Remove
                ShowRemoveTotp = true,
                RemoveTotpLogic = async (pwd) =>
                {
                    await Task.Delay(100);
                    if (pwd != "toor") return TKGenericResult<object>.CreateError("Invalid password");
                    return TKGenericResult.CreateSuccess();
                },
            };
        }

        public override ActionResult Index()
        {
            TKMetricsContext.StartTiming("QoDL.Toolkit.Index()", "QoDL.Toolkit.Index()", true);
            EventSink.RegisterEvent("pageload", new {
                Url = Request.RawUrl,
                User = CurrentRequestInformation?.UserName,
                SettingValue = SettingsService.GetSettings<TestSettings>().IntProp
            });

            var result = base.Index();
            TKMetricsContext.EndAllTimings();
            return result;
        }

        protected override TKFrontEndOptions GetFrontEndOptions()
            => new(EndpointBase)
            {
                ApplicationTitle = "Toolkit",
                ApplicationTitleLink = "/?sysadmin=x&webadmin=1",
                EditorConfig = new TKFrontEndOptions.EditorWorkerConfig
                {
                    //EditorWorkerUrl = "blob:https://unpkg.com/christianw-toolkit@3.0.5/editor.worker.js",
                    //JsonWorkerUrl = "blob:https://unpkg.com/christianw-toolkit@3.0.5/json.worker.js"
                },
                LogoutLinkUrl = $"{EndpointBase}/logout"
            };

        protected override TKPageOptions GetPageOptions()
            => new()
            {
                PageTitle = "Toolkit"
            };

        protected override RequestInformation<RuntimeTestAccessRole> GetRequestInformation(HttpRequestBase request)
        {
            TKMetricsContext.IncrementGlobalCounter("GetRequestInformation()", 1);
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
            if (request.QueryString["notwebadmin"] != null)
            {
                roles &= ~RuntimeTestAccessRole.WebAdmins;
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
        public ActionResult GetMainScript() => LoadFile("toolkit.js");
        [HideFromRequestLog]
        public ActionResult GetMainStyle() => LoadFile("toolkit.css", "text/css");
        
        //[OutputCache(Duration = 1200, VaryByParam = "none")]
        [HideFromRequestLog]
        public ActionResult GetVendorScript() => LoadFile("toolkit.vendor.js");

        [HideFromRequestLog]
        public ActionResult GetMetricsScript() => LoadFile("metrics.js");

        [HideFromRequestLog]
        public ActionResult GetReleaseNotesScript() => LoadFile("releaseNotesSummary.js");

        [HideFromRequestLog]
        public ActionResult GetScript([FromUri]string name) => LoadFile(name);

        private ActionResult LoadFile(string filename, string contentType = "text/plain")
        {
            TKMetricsContext.IncrementGlobalCounter(Path.GetFileName(filename) + ".Load()", 1);
            var filepath = Path.GetFullPath($@"{HostingEnvironment.MapPath("~")}..\..\QoDL.Toolkit.Frontend\dist\{filename}");
            if (!System.IO.File.Exists(filepath)) return Content("");
            return new FileStreamResult(System.IO.File.Open(filepath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite), contentType);
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
            TKMetricsContext.StartTiming("LoginTotal", "Total", addToGlobals: true);

            TKMetricsContext.StartTiming("Login", "Login", addToGlobals: true);
            await Task.Delay(TimeSpan.FromSeconds(0.1));
            var service = new MetricsDummyService();
            if (!(await service.Login()))
            {
                return Content("No");
            }
            TKMetricsContext.EndTiming();

            TKMetricsContext.AddNote("Random value", new Random().Next());
            TKMetricsContext.AddNote("What just happened? 🤔");

            TKMetricsContext.AddGlobalValue("Rng", new Random().Next());

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
                    TestStreamA,
                    testStreamB,
                    testStreamC,
                    simpleStream,
                    memoryStream,
                    otherStream1,
                    otherStream2
                }
            });
            EventSink = new DefaultEventDataSink(EventSinkNotificationConfigStorage, EventSinkNotificationDefinitionStorage)
                .AddNotifier(new TKWebHookEventNotifier())
                .AddNotifier(new MyNotifier())
                .AddNotifier(new SimpleNotifier())
                .AddPlaceholder("NOW", () => DateTimeOffset.Now.ToString())
                .AddPlaceholder("ServerName", () => Environment.MachineName);
            (EventSink as DefaultEventDataSink).IsEnabled = () => SettingsService.GetSettings<TestSettings>().EnableEventRegistering;
        }
        #endregion
    }
}
