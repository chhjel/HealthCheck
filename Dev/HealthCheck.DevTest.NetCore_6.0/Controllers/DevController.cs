using Fido2NetLib;
using HealthCheck.Core.Abstractions;
using HealthCheck.Core.Models;
using HealthCheck.Core.Modules.AccessTokens;
using HealthCheck.Core.Modules.AccessTokens.Abstractions;
using HealthCheck.Core.Modules.AuditLog;
using HealthCheck.Core.Modules.AuditLog.Abstractions;
using HealthCheck.Core.Modules.AuditLog.Models;
using HealthCheck.Core.Modules.Dataflow;
using HealthCheck.Core.Modules.Dataflow.Abstractions;
using HealthCheck.Core.Modules.DataRepeater;
using HealthCheck.Core.Modules.DataRepeater.Abstractions;
using HealthCheck.Core.Modules.Documentation;
using HealthCheck.Core.Modules.Documentation.Services;
using HealthCheck.Core.Modules.EventNotifications;
using HealthCheck.Core.Modules.EventNotifications.Abstractions;
using HealthCheck.Core.Modules.LogViewer;
using HealthCheck.Core.Modules.Messages;
using HealthCheck.Core.Modules.Messages.Abstractions;
using HealthCheck.Core.Modules.Metrics;
using HealthCheck.Core.Modules.Metrics.Abstractions;
using HealthCheck.Core.Modules.Metrics.Context;
using HealthCheck.Core.Modules.ReleaseNotes;
using HealthCheck.Core.Modules.ReleaseNotes.Abstractions;
using HealthCheck.Core.Modules.ReleaseNotes.Models;
using HealthCheck.Core.Modules.SecureFileDownload;
using HealthCheck.Core.Modules.SecureFileDownload.Abstractions;
using HealthCheck.Core.Modules.SecureFileDownload.FileStorage;
using HealthCheck.Core.Modules.Settings;
using HealthCheck.Core.Modules.Settings.Abstractions;
using HealthCheck.Core.Modules.SiteEvents;
using HealthCheck.Core.Modules.SiteEvents.Abstractions;
using HealthCheck.Core.Modules.Tests;
using HealthCheck.Core.Modules.Tests.Models;
using HealthCheck.Core.Util;
using HealthCheck.Core.Util.Modules;
using HealthCheck.Dev.Common;
using HealthCheck.Dev.Common.Dataflow;
using HealthCheck.Dev.Common.Settings;
using HealthCheck.Dev.Common.Tests;
using HealthCheck.Module.DataExport;
using HealthCheck.Module.DataExport.Abstractions;
using HealthCheck.Module.DataExport.Exporter.Excel;
using HealthCheck.Module.DevModule;
using HealthCheck.Module.DynamicCodeExecution.Abstractions;
using HealthCheck.Module.DynamicCodeExecution.Models;
using HealthCheck.Module.DynamicCodeExecution.Module;
using HealthCheck.Module.DynamicCodeExecution.Storage;
using HealthCheck.Module.DynamicCodeExecution.Validators;
using HealthCheck.Module.EndpointControl.Abstractions;
using HealthCheck.Module.EndpointControl.Module;
using HealthCheck.WebUI.Abstractions;
using HealthCheck.WebUI.MFA.TOTP;
using HealthCheck.WebUI.MFA.WebAuthn;
using HealthCheck.WebUI.MFA.WebAuthn.Storage;
using HealthCheck.WebUI.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HealthCheck.DevTest.NetCore_6._0.Controllers
{
    [Route("/")]
    public class DevController : HealthCheckControllerBase<RuntimeTestAccessRole>
    {
        #region Props & Fields
        private readonly IWebHostEnvironment _env;
        private readonly IEventDataSink _eventDataSink;
        private readonly IAuditEventStorage _auditEventStorage;
        private const string EndpointBase = "/";
        private static bool ForceLogout { get; set; }
        #endregion

        public DevController(IWebHostEnvironment env,
            IEndpointControlService endpointControlService,
            IEndpointControlRuleStorage endpointControlRuleStorage,
            IEndpointControlEndpointDefinitionStorage endpointControlEndpointDefinitionStorage,
            IEndpointControlRequestHistoryStorage endpointControlRequestHistoryStorage,
            ISecureFileDownloadDefinitionStorage secureFileDownloadDefinitionStorage,
            IAccessManagerTokenStorage accessManagerTokenStorage,
            IEventDataSink eventDataSink,
            ILogSearcherService logSearcherService,
            IDataflowService<RuntimeTestAccessRole> dataflowService,
            IAuditEventStorage auditEventStorage,
            ISiteEventService siteEventService,
            IHCSettingsService settingsService,
            IHCMetricsStorage metricsStorage,
            IHCDataRepeaterService dataRepeaterService,
            IHCDataExportService dataExportService,
            IHCDataExportPresetStorage dataExportPresetStorage,
            IHCMessageStorage messageStore,
            IHCReleaseNotesProvider releaseNotesProvider
        )
            : base()
        {
            _env = env;
            _eventDataSink = eventDataSink;
            _auditEventStorage = auditEventStorage;

            UseModule(new HCTestsModule(new HCTestsModuleOptions()
            {
                AssembliesContainingTests = new[]
                    {
                        typeof(DevController).Assembly,
                        typeof(RuntimeTestConstants).Assembly
                    },
                ReferenceParameterFactories = CreateReferenceParameterFactories,
                FileDownloadHandler = (type, id) =>
                {
                    if (id == "404") return null;
                    else if (Guid.TryParse(id, out var fileGuid)) return HealthCheckFileDownloadResult.CreateFromString("guid.txt", $"The guid was {id}");
                    else if (id == "ascii") return HealthCheckFileDownloadResult.CreateFromString("Success.txt", $"Type: {type}, Id: {id}. ÆØÅæøå etc ôasd. ASCII", encoding: Encoding.ASCII);
                    else return HealthCheckFileDownloadResult.CreateFromString("Success.txt", $"Type: {type}, Id: {id}. ÆØÅæøå etc ôasd.");
                },
            }))
                .ConfigureGroups((options) => options
                    .ConfigureGroup(RuntimeTestConstants.Group.TopGroup, uiOrder: 130)
                    .ConfigureGroup(RuntimeTestConstants.Group.Modules, uiOrder: 120)
                    .ConfigureGroup(RuntimeTestConstants.Group.AdminStuff, uiOrder: 100)
                    .ConfigureGroup(RuntimeTestConstants.Group.AlmostTopGroup, uiOrder: 50)
                    .ConfigureGroup(RuntimeTestConstants.Group.AlmostBottomGroup, uiOrder: -20)
                    .ConfigureGroup(RuntimeTestConstants.Group.BottomGroup, uiOrder: -50)
                );
            UseModule(new HCMessagesModule(new HCMessagesModuleOptions() { MessageStorage = messageStore }
                .DefineInbox("mail", "Mail", "All sent email ends up here.")
                .DefineInbox("sms", "SMS", "All sent sms ends up here.")
            ));
            UseModule(new HCDataExportModule(new HCDataExportModuleOptions
                {
                    Service = dataExportService,
                    PresetStorage = dataExportPresetStorage
                }
                .AddExporter(new HCDataExportExporterXlsx())
            ));
            UseModule(new HCDataRepeaterModule(new HCDataRepeaterModuleOptions
            {
                Service = dataRepeaterService
            }));
            UseModule(new HCEndpointControlModule(new HCEndpointControlModuleOptions()
            {
                EndpointControlService = endpointControlService,
                RuleStorage = endpointControlRuleStorage,
                DefinitionStorage = endpointControlEndpointDefinitionStorage,
                HistoryStorage = endpointControlRequestHistoryStorage
            }));
            UseModule(new HCMetricsModule(new HCMetricsModuleOptions()
            {
                Storage = metricsStorage
            }));
            UseModule(new HCSecureFileDownloadModule(new HCSecureFileDownloadModuleOptions()
            {
                DefinitionStorage = secureFileDownloadDefinitionStorage,
                FileStorages = new ISecureFileDownloadFileStorage[]
                {
                    new FolderFileStorage("files_testU", "Disk storage (upload only)", @"C:\temp\fileStorageTest") { SupportsSelectingFile = false, SupportsUpload = true },
                    new FolderFileStorage("files_testD", "Disk storage (download only)", @"C:\temp\fileStorageTest") { SupportsSelectingFile = true, SupportsUpload = false },
                    new FolderFileStorage("files_testUD", "Disk storage (upload and download)", @"C:\temp\fileStorageTest") { SupportsSelectingFile = true, SupportsUpload = true },
                    new UrlFileStorage("urls_test", "External url")
                }
            }));
            UseModule(new HCAccessTokensModule(new HCAccessTokensModuleOptions()
            {
                TokenStorage = accessManagerTokenStorage
            }));
            UseModule(new HCEventNotificationsModule(new HCEventNotificationsModuleOptions() { EventSink = eventDataSink }));
            UseModule(new HCLogViewerModule(new HCLogViewerModuleOptions() { LogSearcherService = logSearcherService }));
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
            UseModule(new HCDataflowModule<RuntimeTestAccessRole>(new HCDataflowModuleOptions<RuntimeTestAccessRole>() { DataflowService = dataflowService }));
            UseModule(new HCAuditLogModule(new HCAuditLogModuleOptions() {
                AuditEventService = auditEventStorage,
                SensitiveDataStripper = (value) => {
                    value = HCSensitiveDataUtils.MaskNorwegianNINs(value);
                    value = HCSensitiveDataUtils.MaskAllEmails(value);
                    return value;
                }
            }));
            UseModule(new HCReleaseNotesModule(new HCReleaseNotesModuleOptions
            {
                ReleaseNotesProvider = releaseNotesProvider,
                TopLinks = new List<HCReleaseNoteLinkWithAccess>
                {
                    new HCReleaseNoteLinkWithAccess("Link for anyone", "https://www.google.com?q=some+link+here"),
                    new HCReleaseNoteLinkWithAccess("Link for webadmins", "https://www.google.com?q=some+link+here", RuntimeTestAccessRole.WebAdmins),
                    new HCReleaseNoteLinkWithAccess("Link for sysadmins", "https://www.google.com?q=some+link+here", RuntimeTestAccessRole.SystemAdmins)
                }
            }));
            UseModule(new HCSiteEventsModule(new HCSiteEventsModuleOptions() { SiteEventService = siteEventService, CustomHtml = "<h2>Something custom here</h2><p>And some more.</p>" }));
            UseModule(new HCSettingsModule(new HCSettingsModuleOptions() { Service = settingsService, ModelType = typeof(TestSettings) }));
            UseModule(new HCDynamicCodeExecutionModule(new HCDynamicCodeExecutionModuleOptions()
            {
                StoreCopyOfExecutedScriptsAsAuditBlobs = true,
                TargetAssembly = typeof(DevController).Assembly,
                ScriptStorage = new FlatFileDynamicCodeScriptStorage(@"C:\temp\DCE_Scripts.json"),
                PreProcessors = new IDynamicCodePreProcessor[0],
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
                        .Select(x => new RuntimeTestReferenceParameterChoice(x.Id.ToString(), x.Title, x.Id % 3 == 0 ? $"Id: {x.Id}" : null)),
                    (id) => getUserChoices().FirstOrDefault(x => x.Id.ToString() == id)
                )
            };
        }
        
        #region Overrides
        protected override HCFrontEndOptions GetFrontEndOptions()
            => new(EndpointBase)
            {
                ApplicationTitle = "HealthCheck",
                ApplicationTitleLink = "/?sysadmin=x&webadmin=1",
                EditorConfig = new HCFrontEndOptions.EditorWorkerConfig
                {
                    //EditorWorkerUrl = "blob:https://unpkg.com/christianh-healthcheck@3.0.5/editor.worker.js",
                    //JsonWorkerUrl = "blob:https://unpkg.com/christianh-healthcheck@3.0.5/json.worker.js"
                },
                LogoutLinkUrl = "/Logout"
            };

        protected override HCPageOptions GetPageOptions()
            => new()
            {
                PageTitle = "HealthCheck"
            };

        protected override void ConfigureAccess(HttpRequest request, AccessConfig<RuntimeTestAccessRole> config)
        {
            HCRequestData.IncrementCounter("ConfigureAccessCallCount");
            HCRequestData.SetDetail("Url", request.GetDisplayUrl());
            HCRequestData.AddError("Oh no something failed!");
            HCRequestData.AddError("Oh no another error!", new Exception("Some message here"));
            HCRequestData.AddError("Oh no something failed!", "This one with some manual details.");

            /// MODULES //
            config.GiveRolesAccessToModule(
                RuntimeTestAccessRole.Guest | RuntimeTestAccessRole.WebAdmins,
                TestModuleA.TestModuleAAccessOption.DeleteThing | TestModuleA.TestModuleAAccessOption.EditThing
            );

            config.GiveRolesAccessToModule(RuntimeTestAccessRole.SystemAdmins, TestModuleB.TestModuleBAccessOption.NumberOne);
            
            config.GiveRolesAccessToModuleWithFullAccess<HCReleaseNotesModule>(RuntimeTestAccessRole.WebAdmins);
            config.GiveRolesAccessToModuleWithFullAccess<HCDataRepeaterModule>(RuntimeTestAccessRole.WebAdmins);
            config.GiveRolesAccessToModuleWithFullAccess<HCDataExportModule>(RuntimeTestAccessRole.WebAdmins);
            //config.GiveRolesAccessToModuleWithFullAccess<HCDataExportModule>(RuntimeTestAccessRole.QuerystringTest);
            config.GiveRolesAccessToModuleWithFullAccess<HCTestsModule>(RuntimeTestAccessRole.WebAdmins);
            config.GiveRolesAccessToModuleWithFullAccess<HCSettingsModule>(RuntimeTestAccessRole.WebAdmins);
            config.GiveRolesAccessToModuleWithFullAccess<HCSiteEventsModule>(RuntimeTestAccessRole.WebAdmins);
            //config.GiveRolesAccessToModule(RuntimeTestAccessRole.WebAdmins, HCSiteEventsModule.AccessOption.DeveloperDetails);
            //config.GiveRolesAccessToModule(RuntimeTestAccessRole.WebAdmins, HCSiteEventsModule.AccessOption.None);
            config.GiveRolesAccessToModuleWithFullAccess<HCAuditLogModule>(RuntimeTestAccessRole.WebAdmins);
            config.GiveRolesAccessToModuleWithFullAccess<HCDataflowModule<RuntimeTestAccessRole>>(RuntimeTestAccessRole.WebAdmins);
            config.GiveRolesAccessToModuleWithFullAccess<HCDocumentationModule>(RuntimeTestAccessRole.WebAdmins);
            config.GiveRolesAccessToModuleWithFullAccess<HCLogViewerModule>(RuntimeTestAccessRole.WebAdmins);
            config.GiveRolesAccessToModuleWithFullAccess<HCEventNotificationsModule>(RuntimeTestAccessRole.WebAdmins);
            config.GiveRolesAccessToModuleWithFullAccess<HCAccessTokensModule>(RuntimeTestAccessRole.SystemAdmins);
            config.GiveRolesAccessToModuleWithFullAccess<HCSecureFileDownloadModule>(RuntimeTestAccessRole.WebAdmins);
            config.GiveRolesAccessToModuleWithFullAccess<TestModuleB>(RuntimeTestAccessRole.WebAdmins);
            config.GiveRolesAccessToModuleWithFullAccess<HCEndpointControlModule>(RuntimeTestAccessRole.WebAdmins);
            config.GiveRolesAccessToModuleWithFullAccess<HCMetricsModule>(RuntimeTestAccessRole.WebAdmins);
            config.GiveRolesAccessToModuleWithFullAccess<HCMessagesModule>(RuntimeTestAccessRole.WebAdmins);
            config.GiveRolesAccessToModuleWithFullAccess<HCDynamicCodeExecutionModule>(RuntimeTestAccessRole.WebAdmins);
            //////////////

            config.ShowFailedModuleLoadStackTrace = new Maybe<RuntimeTestAccessRole>(RuntimeTestAccessRole.WebAdmins);
            config.PingAccess = new Maybe<RuntimeTestAccessRole>(RuntimeTestAccessRole.API);
            config.RedirectTargetOnNoAccess = "/no-access";
            //config.RedirectTargetOnNoAccessUsingRequest = (r, q) => $"/DummyLoginRedirect?r={HttpUtility.UrlEncode($"/?{q}")}";
            config.IntegratedLoginConfig = new HCIntegratedLoginConfig("/HCLogin/login")
                //.EnableOneTimePasswordWithCodeRequest("/hclogin/Request2FACode", "Code pls", required: false)
                .EnableTOTP("Optional for higher access level", required: false)
                .EnableWebAuthn("Optional to access more things", required: false)
                ;

            var totpKey = "_dev_totp_secret";
            var webAuthnKey = "_dev_webAuthn_secret";
            config.IntegratedProfileConfig = new HCIntegratedProfileConfig
            {
                Username = CurrentRequestInformation.UserName,
                BodyHtml = "Here is some custom content.<ul><li><a href=\"https://www.google.com\">A link here</a></li></ul>",
                // TOTP: Elevate
                ShowTotpElevation = !string.IsNullOrWhiteSpace(Request.HttpContext.Session.GetString(totpKey))
                    && string.IsNullOrWhiteSpace(Request.HttpContext.Session.GetString("_dev_2fa_validated")),
                CurrentTotpCodeExpirationTime = HCMfaTotpUtil.GetCurrentTotpCodeExpirationTime(),
                TotpElevationLogic = async (c) =>
                {
                    await Task.Delay(100);
                    var secret = Request.HttpContext.Session.GetString(totpKey);
                    if (string.IsNullOrWhiteSpace(secret) || !HCMfaTotpUtil.ValidateTotpCode(secret, c))
                    {
                        return HCGenericResult<HCResultPageAction>.CreateError("Invalid code");
                    }
                    Request.HttpContext.Session.SetString("_dev_2fa_validated", "true");
                    
                    return HCGenericResult<HCResultPageAction>.CreateSuccess(HCResultPageAction.CreateRefresh());
                },
                // TOTP: Add
                ShowAddTotp = string.IsNullOrWhiteSpace(Request.HttpContext.Session.GetString(totpKey)),
                AddTotpLogic = async (pwd, secret, code) =>
                {
                    await Task.Delay(100);
                    if (pwd != "toor") return HCGenericResult<object>.CreateError("Invalid password");
                    else if (!string.IsNullOrWhiteSpace(Request.HttpContext.Session.GetString(totpKey)))
                    {
                        return HCGenericResult.CreateError("TOTP already activated.");
                    }
                    else if (!HCMfaTotpUtil.ValidateTotpCode(secret, code))
                    {
                        return HCGenericResult.CreateError("Invalid code");
                    }
                    Request.HttpContext.Session.SetString(totpKey, secret);
                    return HCGenericResult.CreateSuccess();
                },
                // TOTP: Remove
                ShowRemoveTotp = !string.IsNullOrWhiteSpace(Request.HttpContext.Session.GetString(totpKey)),
                RemoveTotpLogic = async (pwd) =>
                {
                    await Task.Delay(100);
                    if (pwd != "toor") return HCGenericResult<object>.CreateError("Invalid password");
                    else if (string.IsNullOrWhiteSpace(Request.HttpContext.Session.GetString(totpKey)))
                    {
                        return HCGenericResult.CreateError("TOTP already removed.");
                    }
                    Request.HttpContext.Session.Remove(totpKey);
                    Request.HttpContext.Session.Remove("_dev_2fa_validated");
                    return HCGenericResult.CreateSuccess();
                },
                
                // WebAuthn: Elevate
                ShowWebAuthnElevation = !string.IsNullOrWhiteSpace(Request.HttpContext.Session.GetString(webAuthnKey))
                    && string.IsNullOrWhiteSpace(Request.HttpContext.Session.GetString("_dev_webAuthn_validated")),
                CreateWebAuthnAssertionOptionsLogic = async (u) =>
                {
                    await Task.Delay(100);
                    var webauthn = CreateWebAuthnHelper();
                    var options = webauthn.CreateAssertionOptions(u);
                    if (options == null)
                    {
                        return HCGenericResult<object>.CreateError($"User not found.");
                    }

                    HttpContext.Session.SetString("WebAuthn.assertionOptionsDev", JsonConvert.SerializeObject(options));
                    return HCGenericResult<object>.CreateSuccess(options);
                },
                WebAuthnElevationLogic = async (d) =>
                {
                    await Task.Delay(100);
                    var webauthn = CreateWebAuthnHelper();
                    var jsonOptions = HttpContext.Session.GetString("WebAuthn.assertionOptionsDev");
                    var options = AssertionOptions.FromJson(jsonOptions);
                    var webAuthnResult = HCAsyncUtils.RunSync(() => webauthn.VerifyAssertion(options, d));
                    if (!webAuthnResult.Success)
                    {
                        return HCGenericResult<HCResultPageAction>.CreateError(webAuthnResult.Error);
                    }
                    Request.HttpContext.Session.SetString("_dev_webAuthn_validated", "true");
                    return HCGenericResult<HCResultPageAction>.CreateSuccess(HCResultPageAction.CreateRefresh());
                },
                // WebAuthn: Add
                ShowAddWebAuthn = string.IsNullOrWhiteSpace(Request.HttpContext.Session.GetString(webAuthnKey)),
                CreateWebAuthnRegistrationOptionsLogic = async (username, pwd) =>
                {
                    await Task.Delay(100);
                    if (pwd != "toor") return HCGenericResult<object>.CreateError("Invalid password");
                    var webauthn = CreateWebAuthnHelper();
                    var options = webauthn.CreateClientOptions(username);
                    HttpContext.Session.SetString("WebAuthn.attestationOptions", options.ToJson());
                    return HCGenericResult<object>.CreateSuccess(options);
                },
                AddWebAuthnLogic = async (pwd, attestation) =>
                {
                    await Task.Delay(100);
                    if (pwd != "toor") return HCGenericResult<object>.CreateError("Invalid password");
                    var webauthn = CreateWebAuthnHelper();
                    var jsonOptions = HttpContext.Session.GetString("WebAuthn.attestationOptions");
                    var options = CredentialCreateOptions.FromJson(jsonOptions);

                    HCAsyncUtils.RunSync(() => webauthn.RegisterCredentials(options, attestation));
                    Request.HttpContext.Session.SetString(webAuthnKey, "added");
                    return HCGenericResult.CreateSuccess();
                },
                // WebAuthn: Remove
                ShowRemoveWebAuthn = !string.IsNullOrWhiteSpace(Request.HttpContext.Session.GetString(webAuthnKey)),
                RemoveWebAuthnLogic = async (pwd) =>
                {
                    await Task.Delay(100);
                    if (pwd != "toor") return HCGenericResult<object>.CreateError("Invalid password");
                    else if (string.IsNullOrWhiteSpace(Request.HttpContext.Session.GetString(webAuthnKey)))
                    {
                        return HCGenericResult.CreateError("WebAuthn already removed.");
                    }
                    Request.HttpContext.Session.Remove(webAuthnKey);
                    Request.HttpContext.Session.Remove("_dev_webAuthn_validated");
                    return HCGenericResult.CreateSuccess();
                }
            };
        }

        protected override RequestInformation<RuntimeTestAccessRole> GetRequestInformation(HttpRequest request)
        {
            HCRequestData.IncrementCounter("GetRequestInformationCallCount");
            HCRequestData.SetDetail("Url", request.GetDisplayUrl());
            HCRequestData.SetDetail("Path", Request?.Path);
            _eventDataSink.RegisterEvent("GetRequestInfo", new
            {
                Type = GetType().Name,
                Path = Request?.Path,
                Message = "wut"
            });
            HCMetricsContext.AddNote("Random value", new Random().Next());
            HCMetricsContext.AddGlobalValue("Rng", new Random().Next());
            HCMetricsContext.IncrementGlobalCounter("TestCounter");
            HCMetricsContext.AddGlobalNote("GlobalNoteTest", $"rng is {new Random().Next()}");

            if (ForcedRole != null)
            {
                return new RequestInformation<RuntimeTestAccessRole>(ForcedRole.Value, "forcedId", "Forced role user");
            }

            var roles = RuntimeTestAccessRole.Guest;
            if (request.Query.ContainsKey("qstest"))
            {
                roles |= RuntimeTestAccessRole.QuerystringTest;
            }
            if (request.Query.ContainsKey("makeABlob"))
            {
                _auditEventStorage.StoreEvent(new AuditEvent(DateTimeOffset.Now, "DevArea", "Dev test title", "Subject", "UserX", "User X", new List<string>())
                    .AddBlob("Blob name", "Blob contents here."));
            }

            if (request.Query.ContainsKey("noaccess"))
            {
                roles = RuntimeTestAccessRole.None;
                return new RequestInformation<RuntimeTestAccessRole>(roles, "no_access_test", "No user");
            }

            if (request.Query.ContainsKey("dataflowABC"))
            {
                Config.IoCConfig.TestStreamA.InsertEntries(Enumerable.Range(1, 5000).Select(i => new TestEntry { Code = $"100{i}-A", Name = $"Entry A{i} [{DateTimeOffset.Now}]" }));
                Config.IoCConfig.TestStreamB.InsertEntries(Enumerable.Range(1, 5000).Select(i => new TestEntry { Code = $"200{i}-B", Name = $"Entry B{i} [{DateTimeOffset.Now}]" }));
                Config.IoCConfig.TestStreamC.InsertEntries(Enumerable.Range(1, 5000).Select(i => new TestEntry { Code = $"300{i}-C", Name = $"Entry C{i} [{DateTimeOffset.Now}]" }));
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
            if (request.Query.ContainsKey("notwebadmin"))
            {
                roles &= ~RuntimeTestAccessRole.WebAdmins;
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
        private HCWebAuthnHelper CreateWebAuthnHelper()
            => new(new HCWebAuthnHelperOptions
            {
                ServerDomain = "localhost",
                ServerName = "HCDev",
                Origin = Request.Headers["Origin"]
            }, new HCMemoryWebAuthnCredentialManager());

        [Route("GetMainScript")]
        public ActionResult GetMainScript() => LoadFile("healthcheck.js");

        [Route("GetMainStyle")]
        public ActionResult GetMainStyle() => LoadFile("healthcheck.css", "text/css");

        [Route("GetVendorScript")]
        public ActionResult GetVendorScript() => LoadFile("healthcheck.vendor.js");

        [Route("GetMetricsScript")]
        public ActionResult GetMetricsScript() => LoadFile("metrics.js");

        [Route("GetReleaseNotesScript")]
        public ActionResult GetReleaseNotesScript() => LoadFile("releaseNotesSummary.js");

        [Route("GetScript")]
        public ActionResult GetScript(string name) => LoadFile(name);

        private ActionResult LoadFile(string filename, string contentType = "text/plain")
        {
            var filepath = GetFilePath($@"..\..\HealthCheck.Frontend\dist\{filename}");
            if (!System.IO.File.Exists(filepath)) return NotFound();
            return new FileStreamResult(new FileStream(filepath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite), new MediaTypeHeaderValue(contentType))
            {
                FileDownloadName = Path.GetFileName(filepath)
            };
        }

        private string GetFilePath(string relativePath) => Path.GetFullPath(Path.Combine(_env.ContentRootPath, relativePath));

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
        public ActionResult ForceAccessRole(string? name = null)
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

        // New mock data
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

            Config.IoCConfig.TestStreamA.InsertEntries(entriesToInsert);

            return Content("OK :]");
        }
        #endregion
    }
}
