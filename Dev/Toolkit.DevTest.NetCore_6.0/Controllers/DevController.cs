using Fido2NetLib;
using QoDL.Toolkit.Core.Abstractions;
using QoDL.Toolkit.Core.Models;
using QoDL.Toolkit.Core.Modules.AccessTokens;
using QoDL.Toolkit.Core.Modules.AccessTokens.Abstractions;
using QoDL.Toolkit.Core.Modules.AuditLog;
using QoDL.Toolkit.Core.Modules.AuditLog.Abstractions;
using QoDL.Toolkit.Core.Modules.AuditLog.Models;
using QoDL.Toolkit.Core.Modules.Comparison;
using QoDL.Toolkit.Core.Modules.Comparison.Abstractions;
using QoDL.Toolkit.Core.Modules.ContentPermutation;
using QoDL.Toolkit.Core.Modules.ContentPermutation.Abstractions;
using QoDL.Toolkit.Core.Modules.Dataflow;
using QoDL.Toolkit.Core.Modules.Dataflow.Abstractions;
using QoDL.Toolkit.Core.Modules.DataRepeater;
using QoDL.Toolkit.Core.Modules.DataRepeater.Abstractions;
using QoDL.Toolkit.Core.Modules.Documentation;
using QoDL.Toolkit.Core.Modules.Documentation.Services;
using QoDL.Toolkit.Core.Modules.EventNotifications;
using QoDL.Toolkit.Core.Modules.EventNotifications.Abstractions;
using QoDL.Toolkit.Core.Modules.GoTo;
using QoDL.Toolkit.Core.Modules.GoTo.Abstractions;
using QoDL.Toolkit.Core.Modules.Jobs;
using QoDL.Toolkit.Core.Modules.Jobs.Abstractions;
using QoDL.Toolkit.Core.Modules.LogViewer;
using QoDL.Toolkit.Core.Modules.MappedData;
using QoDL.Toolkit.Core.Modules.MappedData.Abstractions;
using QoDL.Toolkit.Core.Modules.Messages;
using QoDL.Toolkit.Core.Modules.Messages.Abstractions;
using QoDL.Toolkit.Core.Modules.Metrics;
using QoDL.Toolkit.Core.Modules.Metrics.Abstractions;
using QoDL.Toolkit.Core.Modules.Metrics.Context;
using QoDL.Toolkit.Core.Modules.ReleaseNotes;
using QoDL.Toolkit.Core.Modules.ReleaseNotes.Abstractions;
using QoDL.Toolkit.Core.Modules.ReleaseNotes.Models;
using QoDL.Toolkit.Core.Modules.SecureFileDownload;
using QoDL.Toolkit.Core.Modules.SecureFileDownload.Abstractions;
using QoDL.Toolkit.Core.Modules.SecureFileDownload.FileStorage;
using QoDL.Toolkit.Core.Modules.Settings;
using QoDL.Toolkit.Core.Modules.Settings.Abstractions;
using QoDL.Toolkit.Core.Modules.SiteEvents;
using QoDL.Toolkit.Core.Modules.SiteEvents.Abstractions;
using QoDL.Toolkit.Core.Modules.Tests;
using QoDL.Toolkit.Core.Modules.Tests.Models;
using QoDL.Toolkit.Core.Util;
using QoDL.Toolkit.Core.Util.Modules;
using QoDL.Toolkit.Dev.Common;
using QoDL.Toolkit.Dev.Common.Dataflow;
using QoDL.Toolkit.Dev.Common.Settings;
using QoDL.Toolkit.Dev.Common.Tests;
using QoDL.Toolkit.Module.DataExport;
using QoDL.Toolkit.Module.DataExport.Abstractions;
using QoDL.Toolkit.Module.DataExport.Exporter.Excel;
using QoDL.Toolkit.Module.DevModule;
using QoDL.Toolkit.Module.DynamicCodeExecution.Abstractions;
using QoDL.Toolkit.Module.DynamicCodeExecution.Models;
using QoDL.Toolkit.Module.DynamicCodeExecution.Module;
using QoDL.Toolkit.Module.DynamicCodeExecution.Storage;
using QoDL.Toolkit.Module.DynamicCodeExecution.Validators;
using QoDL.Toolkit.Module.EndpointControl.Abstractions;
using QoDL.Toolkit.Module.EndpointControl.Module;
using QoDL.Toolkit.WebUI.Abstractions;
using QoDL.Toolkit.WebUI.MFA.TOTP;
using QoDL.Toolkit.WebUI.MFA.WebAuthn;
using QoDL.Toolkit.WebUI.MFA.WebAuthn.Storage;
using QoDL.Toolkit.WebUI.Models;
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

namespace QoDL.Toolkit.DevTest.NetCore_6._0.Controllers
{
    [Route("/")]
    public class DevController : ToolkitControllerBase<RuntimeTestAccessRole>
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
            ITKSettingsService settingsService,
            ITKMetricsStorage metricsStorage,
            ITKDataRepeaterService dataRepeaterService,
            ITKDataExportService dataExportService,
            ITKDataExportPresetStorage dataExportPresetStorage,
            ITKMessageStorage messageStore,
            ITKReleaseNotesProvider releaseNotesProvider,
            ITKContentPermutationContentDiscoveryService permutationContentDiscoveryService,
            ITKComparisonService comparisonService,
            ITKGoToService goToService,
            ITKMappedDataService mappeddataService,
            ITKJobsService jobsService
        )
            : base()
        {
            _env = env;
            _eventDataSink = eventDataSink;
            _auditEventStorage = auditEventStorage;

            UseModule(new TKTestsModule(new TKTestsModuleOptions()
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
                    else if (Guid.TryParse(id, out var fileGuid)) return ToolkitFileDownloadResult.CreateFromString("guid.txt", $"The guid was {id}");
                    else if (id == "ascii") return ToolkitFileDownloadResult.CreateFromString("Success.txt", $"Type: {type}, Id: {id}. ������ etc �asd. ASCII", encoding: Encoding.ASCII);
                    else return ToolkitFileDownloadResult.CreateFromString("Success.txt", $"Type: {type}, Id: {id}. ������ etc �asd.");
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
            UseModule(new TKJobsModule(new TKJobsModuleOptions
            {
                Service = jobsService
            }));
            UseModule(new TKMappedDataModule(new TKMappedDataModuleOptions
            {
                Service = mappeddataService,
                IncludedAssemblies = new[]
                {
                    typeof(DevController).Assembly,
                    typeof(RuntimeTestConstants).Assembly
                }
            }));
            UseModule(new TKGoToModule(new TKGoToModuleOptions
            {
                Service = goToService
            }));
            UseModule(new TKComparisonModule(new TKComparisonModuleOptions
            {
                Service = comparisonService
            }));
            UseModule(new TKContentPermutationModule(new TKContentPermutationModuleOptions
            {
                AssembliesContainingPermutationTypes = new[]
                    {
                        typeof(DevController).Assembly,
                        typeof(RuntimeTestConstants).Assembly
                    },
                Service = permutationContentDiscoveryService
            }));
            UseModule(new TKMessagesModule(new TKMessagesModuleOptions() { MessageStorage = messageStore }
                .DefineInbox("mail", "Mail", "All sent email ends up here.")
                .DefineInbox("sms", "SMS", "All sent sms ends up here.")
            ));
            UseModule(new TKDataExportModule(new TKDataExportModuleOptions
                {
                    Service = dataExportService,
                    PresetStorage = dataExportPresetStorage
                }
                .AddExporter(new TKDataExportExporterXlsx())
            ));
            UseModule(new TKDataRepeaterModule(new TKDataRepeaterModuleOptions
            {
                Service = dataRepeaterService
            }));
            UseModule(new TKEndpointControlModule(new TKEndpointControlModuleOptions()
            {
                EndpointControlService = endpointControlService,
                RuleStorage = endpointControlRuleStorage,
                DefinitionStorage = endpointControlEndpointDefinitionStorage,
                HistoryStorage = endpointControlRequestHistoryStorage
            }));
            UseModule(new TKMetricsModule(new TKMetricsModuleOptions()
            {
                Storage = metricsStorage
            }));
            UseModule(new TKSecureFileDownloadModule(new TKSecureFileDownloadModuleOptions()
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
            UseModule(new TKAccessTokensModule(new TKAccessTokensModuleOptions()
            {
                TokenStorage = accessManagerTokenStorage
            }));
            UseModule(new TKEventNotificationsModule(new TKEventNotificationsModuleOptions() { EventSink = eventDataSink }));
            UseModule(new TKLogViewerModule(new TKLogViewerModuleOptions() { LogSearcherService = logSearcherService }));
            UseModule(new TKDocumentationModule(new TKDocumentationModuleOptions()
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
            UseModule(new TKDataflowModule<RuntimeTestAccessRole>(new TKDataflowModuleOptions<RuntimeTestAccessRole>() { DataflowService = dataflowService }));
            UseModule(new TKAuditLogModule(new TKAuditLogModuleOptions() {
                AuditEventService = auditEventStorage,
                SensitiveDataStripper = (value) => {
                    value = TKSensitiveDataUtils.MaskNorwegianNINs(value);
                    value = TKSensitiveDataUtils.MaskAllEmails(value);
                    return value;
                }
            }));
            UseModule(new TKReleaseNotesModule(new TKReleaseNotesModuleOptions
            {
                ReleaseNotesProvider = releaseNotesProvider,
                TopLinks = new List<TKReleaseNoteLinkWithAccess>
                {
                    new TKReleaseNoteLinkWithAccess("Link for anyone", "https://www.google.com?q=some+link+here"),
                    new TKReleaseNoteLinkWithAccess("Link for webadmins", "https://www.google.com?q=some+link+here", RuntimeTestAccessRole.WebAdmins),
                    new TKReleaseNoteLinkWithAccess("Link for sysadmins", "https://www.google.com?q=some+link+here", RuntimeTestAccessRole.SystemAdmins)
                }
            }));
            UseModule(new TKSiteEventsModule(new TKSiteEventsModuleOptions() { SiteEventService = siteEventService, CustomHtml = "<h2>Something custom here</h2><p>And some more.</p>" }));
            UseModule(new TKSettingsModule(new TKSettingsModuleOptions() { Service = settingsService, ModelType = typeof(TestSettings) }));
            UseModule(new TKDynamicCodeExecutionModule(new TKDynamicCodeExecutionModuleOptions()
            {
                StoreCopyOfExecutedScriptsAsAuditBlobs = true,
                TargetAssembly = typeof(DevController).Assembly,
                ScriptStorage = new FlatFileDynamicCodeScriptStorage(@"C:\temp\DCE_Scripts.json"),
                PreProcessors = Array.Empty<IDynamicCodePreProcessor>(),
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
                LogoutLinkUrl = "/Logout"
            };

        protected override TKPageOptions GetPageOptions()
            => new()
            {
                PageTitle = "Toolkit"
            };

        protected override void ConfigureAccess(HttpRequest request, AccessConfig<RuntimeTestAccessRole> config)
        {
            TKRequestData.IncrementCounter("ConfigureAccessCallCount");
            TKRequestData.SetDetail("Url", request.GetDisplayUrl());
            TKRequestData.AddError("Oh no something failed!");
            TKRequestData.AddError("Oh no another error!", new Exception("Some message here"));
            TKRequestData.AddError("Oh no something failed!", "This one with some manual details.");

            /// MODULES //
            config.GiveRolesAccessToModule(
                RuntimeTestAccessRole.Guest | RuntimeTestAccessRole.WebAdmins,
                TestModuleA.TestModuleAAccessOption.DeleteThing | TestModuleA.TestModuleAAccessOption.EditThing
            );

            config.GiveRolesAccessToModule(RuntimeTestAccessRole.SystemAdmins, TestModuleB.TestModuleBAccessOption.NumberOne);

            config.GiveRolesAccessToModuleWithFullAccess<TKJobsModule>(RuntimeTestAccessRole.WebAdmins);
            config.GiveRolesAccessToModuleWithFullAccess<TKMappedDataModule>(RuntimeTestAccessRole.WebAdmins);
            config.GiveRolesAccessToModuleWithFullAccess<TKReleaseNotesModule>(RuntimeTestAccessRole.WebAdmins);
            config.GiveRolesAccessToModuleWithFullAccess<TKDataRepeaterModule>(RuntimeTestAccessRole.WebAdmins);
            config.GiveRolesAccessToModuleWithFullAccess<TKDataExportModule>(RuntimeTestAccessRole.WebAdmins);
            config.GiveRolesAccessToModuleWithFullAccess<TKComparisonModule>(RuntimeTestAccessRole.WebAdmins);
            config.GiveRolesAccessToModuleWithFullAccess<TKGoToModule>(RuntimeTestAccessRole.WebAdmins);
            config.GiveRolesAccessToModuleWithFullAccess<TKContentPermutationModule>(RuntimeTestAccessRole.WebAdmins);
            //config.GiveRolesAccessToModuleWithFullAccess<TKDataExportModule>(RuntimeTestAccessRole.QuerystringTest);
            config.GiveRolesAccessToModuleWithFullAccess<TKTestsModule>(RuntimeTestAccessRole.WebAdmins);
            config.GiveRolesAccessToModuleWithFullAccess<TKSettingsModule>(RuntimeTestAccessRole.WebAdmins);
            config.GiveRolesAccessToModuleWithFullAccess<TKSiteEventsModule>(RuntimeTestAccessRole.WebAdmins);
            //config.GiveRolesAccessToModule(RuntimeTestAccessRole.WebAdmins, TKSiteEventsModule.AccessOption.DeveloperDetails);
            //config.GiveRolesAccessToModule(RuntimeTestAccessRole.WebAdmins, TKSiteEventsModule.AccessOption.None);
            config.GiveRolesAccessToModuleWithFullAccess<TKAuditLogModule>(RuntimeTestAccessRole.WebAdmins);
            config.GiveRolesAccessToModuleWithFullAccess<TKDataflowModule<RuntimeTestAccessRole>>(RuntimeTestAccessRole.WebAdmins);
            config.GiveRolesAccessToModuleWithFullAccess<TKDocumentationModule>(RuntimeTestAccessRole.WebAdmins);
            config.GiveRolesAccessToModuleWithFullAccess<TKLogViewerModule>(RuntimeTestAccessRole.WebAdmins);
            config.GiveRolesAccessToModuleWithFullAccess<TKEventNotificationsModule>(RuntimeTestAccessRole.WebAdmins);
            config.GiveRolesAccessToModuleWithFullAccess<TKAccessTokensModule>(RuntimeTestAccessRole.SystemAdmins);
            config.GiveRolesAccessToModuleWithFullAccess<TKSecureFileDownloadModule>(RuntimeTestAccessRole.WebAdmins);
            config.GiveRolesAccessToModuleWithFullAccess<TestModuleB>(RuntimeTestAccessRole.WebAdmins);
            config.GiveRolesAccessToModuleWithFullAccess<TKEndpointControlModule>(RuntimeTestAccessRole.WebAdmins);
            config.GiveRolesAccessToModuleWithFullAccess<TKMetricsModule>(RuntimeTestAccessRole.WebAdmins);
            config.GiveRolesAccessToModuleWithFullAccess<TKMessagesModule>(RuntimeTestAccessRole.WebAdmins);
            config.GiveRolesAccessToModuleWithFullAccess<TKDynamicCodeExecutionModule>(RuntimeTestAccessRole.WebAdmins);
            //////////////

            config.ShowFailedModuleLoadStackTrace = new Maybe<RuntimeTestAccessRole>(RuntimeTestAccessRole.WebAdmins);
            config.PingAccess = new Maybe<RuntimeTestAccessRole>(RuntimeTestAccessRole.API);
            config.RedirectTargetOnNoAccess = "/no-access";
            //config.RedirectTargetOnNoAccessUsingRequest = (r, q) => $"/DummyLoginRedirect?r={HttpUtility.UrlEncode($"/?{q}")}";
            config.IntegratedLoginConfig = new TKIntegratedLoginConfig("/TKLogin/login")
                //.EnableOneTimePasswordWithCodeRequest("/tklogin/Request2FACode", "Code pls", required: false)
                .EnableTOTP("Optional for higher access level", required: false)
                .EnableWebAuthn("Optional to access more things", required: false)
                ;

            var totpKey = "_dev_totp_secret";
            var webAuthnKey = "_dev_webAuthn_secret";
            config.IntegratedProfileConfig = new TKIntegratedProfileConfig
            {
                Username = CurrentRequestInformation.UserName,
                BodyHtml = "Here is some custom content.<ul><li><a href=\"https://www.google.com\">A link here</a></li></ul>",
                // TOTP: Elevate
                ShowTotpElevation = !string.IsNullOrWhiteSpace(Request.HttpContext.Session.GetString(totpKey))
                    && string.IsNullOrWhiteSpace(Request.HttpContext.Session.GetString("_dev_2fa_validated")),
                CurrentTotpCodeExpirationTime = TKMfaTotpUtil.GetCurrentTotpCodeExpirationTime(),
                TotpElevationLogic = async (c) =>
                {
                    await Task.Delay(100);
                    var secret = Request.HttpContext.Session.GetString(totpKey);
                    if (string.IsNullOrWhiteSpace(secret) || !TKMfaTotpUtil.ValidateTotpCode(secret, c))
                    {
                        return TKGenericResult<TKResultPageAction>.CreateError("Invalid code");
                    }
                    Request.HttpContext.Session.SetString("_dev_2fa_validated", "true");
                    
                    return TKGenericResult<TKResultPageAction>.CreateSuccess(TKResultPageAction.CreateRefresh());
                },
                // TOTP: Add
                ShowAddTotp = string.IsNullOrWhiteSpace(Request.HttpContext.Session.GetString(totpKey)),
                AddTotpLogic = async (pwd, secret, code) =>
                {
                    await Task.Delay(100);
                    if (pwd != "toor") return TKGenericResult<object>.CreateError("Invalid password");
                    else if (!string.IsNullOrWhiteSpace(Request.HttpContext.Session.GetString(totpKey)))
                    {
                        return TKGenericResult.CreateError("TOTP already activated.");
                    }
                    else if (!TKMfaTotpUtil.ValidateTotpCode(secret, code))
                    {
                        return TKGenericResult.CreateError("Invalid code");
                    }
                    Request.HttpContext.Session.SetString(totpKey, secret);
                    return TKGenericResult.CreateSuccess();
                },
                // TOTP: Remove
                ShowRemoveTotp = !string.IsNullOrWhiteSpace(Request.HttpContext.Session.GetString(totpKey)),
                RemoveTotpLogic = async (pwd) =>
                {
                    await Task.Delay(100);
                    if (pwd != "toor") return TKGenericResult<object>.CreateError("Invalid password");
                    else if (string.IsNullOrWhiteSpace(Request.HttpContext.Session.GetString(totpKey)))
                    {
                        return TKGenericResult.CreateError("TOTP already removed.");
                    }
                    Request.HttpContext.Session.Remove(totpKey);
                    Request.HttpContext.Session.Remove("_dev_2fa_validated");
                    return TKGenericResult.CreateSuccess();
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
                        return TKGenericResult<object>.CreateError($"User not found.");
                    }

                    HttpContext.Session.SetString("WebAuthn.assertionOptionsDev", JsonConvert.SerializeObject(options));
                    return TKGenericResult<object>.CreateSuccess(options);
                },
                WebAuthnElevationLogic = async (d) =>
                {
                    await Task.Delay(100);
                    var webauthn = CreateWebAuthnHelper();
                    var jsonOptions = HttpContext.Session.GetString("WebAuthn.assertionOptionsDev");
                    var options = AssertionOptions.FromJson(jsonOptions);
                    var webAuthnResult = TKAsyncUtils.RunSync(() => webauthn.VerifyAssertion(options, d));
                    if (!webAuthnResult.Success)
                    {
                        return TKGenericResult<TKResultPageAction>.CreateError(webAuthnResult.Error);
                    }
                    Request.HttpContext.Session.SetString("_dev_webAuthn_validated", "true");
                    return TKGenericResult<TKResultPageAction>.CreateSuccess(TKResultPageAction.CreateRefresh());
                },
                // WebAuthn: Add
                ShowAddWebAuthn = string.IsNullOrWhiteSpace(Request.HttpContext.Session.GetString(webAuthnKey)),
                CreateWebAuthnRegistrationOptionsLogic = async (username, pwd) =>
                {
                    await Task.Delay(100);
                    if (pwd != "toor") return TKGenericResult<object>.CreateError("Invalid password");
                    var webauthn = CreateWebAuthnHelper();
                    var options = webauthn.CreateClientOptions(username);
                    HttpContext.Session.SetString("WebAuthn.attestationOptions", options.ToJson());
                    return TKGenericResult<object>.CreateSuccess(options);
                },
                AddWebAuthnLogic = async (pwd, attestation) =>
                {
                    await Task.Delay(100);
                    if (pwd != "toor") return TKGenericResult<object>.CreateError("Invalid password");
                    var webauthn = CreateWebAuthnHelper();
                    var jsonOptions = HttpContext.Session.GetString("WebAuthn.attestationOptions");
                    var options = CredentialCreateOptions.FromJson(jsonOptions);

                    TKAsyncUtils.RunSync(() => webauthn.RegisterCredentials(options, attestation));
                    Request.HttpContext.Session.SetString(webAuthnKey, "added");
                    return TKGenericResult.CreateSuccess();
                },
                // WebAuthn: Remove
                ShowRemoveWebAuthn = !string.IsNullOrWhiteSpace(Request.HttpContext.Session.GetString(webAuthnKey)),
                RemoveWebAuthnLogic = async (pwd) =>
                {
                    await Task.Delay(100);
                    if (pwd != "toor") return TKGenericResult<object>.CreateError("Invalid password");
                    else if (string.IsNullOrWhiteSpace(Request.HttpContext.Session.GetString(webAuthnKey)))
                    {
                        return TKGenericResult.CreateError("WebAuthn already removed.");
                    }
                    Request.HttpContext.Session.Remove(webAuthnKey);
                    Request.HttpContext.Session.Remove("_dev_webAuthn_validated");
                    return TKGenericResult.CreateSuccess();
                }
            };
        }

        protected override RequestInformation<RuntimeTestAccessRole> GetRequestInformation(HttpRequest request)
        {
            TKRequestData.IncrementCounter("GetRequestInformationCallCount");
            TKRequestData.SetDetail("Url", request.GetDisplayUrl());
            TKRequestData.SetDetail("Path", Request?.Path);
            _eventDataSink.RegisterEvent("GetRequestInfo", new
            {
                Type = GetType().Name,
                Path = Request?.Path,
                Message = "wut"
            });
            TKMetricsContext.AddNote("Random value", new Random().Next());
            TKMetricsContext.AddGlobalValue("Rng", new Random().Next());
            TKMetricsContext.IncrementGlobalCounter("TestCounter");
            TKMetricsContext.AddGlobalNote("GlobalNoteTest", $"rng is {new Random().Next()}");

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
        private TKWebAuthnHelper CreateWebAuthnHelper()
            => new(new TKWebAuthnHelperOptions
            {
                ServerDomain = "localhost",
                ServerName = "TKDev",
                Origin = Request.Headers["Origin"]
            }, new TKMemoryWebAuthnCredentialManager());

        [Route("GetMainScript")]
        public ActionResult GetMainScript() => LoadFile("toolkit.js");

        [Route("GetMainStyle")]
        public ActionResult GetMainStyle() => LoadFile("toolkit.css", "text/css");

        [Route("GetVendorScript")]
        public ActionResult GetVendorScript() => LoadFile("toolkit.vendor.js");

        [Route("GetMetricsScript")]
        public ActionResult GetMetricsScript() => LoadFile("metrics.js");

        [Route("GetReleaseNotesScript")]
        public ActionResult GetReleaseNotesScript() => LoadFile("releaseNotesSummary.js");

        [Route("GetScript")]
        public ActionResult GetScript(string name) => LoadFile(name);

        private ActionResult LoadFile(string filename, string contentType = "text/plain")
        {
            var filepath = GetFilePath($@"..\..\QoDL.Toolkit.Frontend\dist\{filename}");
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
