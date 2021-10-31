using Fido2NetLib;
using HealthCheck.Core.Abstractions;
using HealthCheck.Core.Extensions;
using HealthCheck.Core.Models;
using HealthCheck.Core.Modules.AccessTokens;
using HealthCheck.Core.Modules.AccessTokens.Abstractions;
using HealthCheck.Core.Modules.AuditLog;
using HealthCheck.Core.Modules.AuditLog.Abstractions;
using HealthCheck.Core.Modules.Dataflow;
using HealthCheck.Core.Modules.Dataflow.Abstractions;
using HealthCheck.Core.Modules.Documentation;
using HealthCheck.Core.Modules.Documentation.Services;
using HealthCheck.Core.Modules.EventNotifications;
using HealthCheck.Core.Modules.EventNotifications.Abstractions;
using HealthCheck.Core.Modules.LogViewer;
using HealthCheck.Core.Modules.Metrics;
using HealthCheck.Core.Modules.Metrics.Abstractions;
using HealthCheck.Core.Modules.Metrics.Context;
using HealthCheck.Core.Modules.SecureFileDownload;
using HealthCheck.Core.Modules.SecureFileDownload.Abstractions;
using HealthCheck.Core.Modules.SecureFileDownload.FileStorage;
using HealthCheck.Core.Modules.Settings;
using HealthCheck.Core.Modules.Settings.Abstractions;
using HealthCheck.Core.Modules.SiteEvents;
using HealthCheck.Core.Modules.SiteEvents.Abstractions;
using HealthCheck.Core.Modules.SiteEvents.Enums;
using HealthCheck.Core.Modules.SiteEvents.Models;
using HealthCheck.Core.Modules.SiteEvents.Utils;
using HealthCheck.Core.Modules.Tests;
using HealthCheck.Core.Modules.Tests.Models;
using HealthCheck.Core.Util;
using HealthCheck.Dev.Common;
using HealthCheck.Dev.Common.Dataflow;
using HealthCheck.Dev.Common.Settings;
using HealthCheck.Dev.Common.Tests;
using HealthCheck.Module.DevModule;
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
using System.Threading.Tasks;

namespace HealthCheck.DevTest.NetCore_3._1.Controllers
{
    [Route("/")]
    public class DevController : HealthCheckControllerBase<RuntimeTestAccessRole>
    {
        #region Props & Fields
        private readonly IWebHostEnvironment _env;
        private readonly IEventDataSink _eventDataSink;
        private readonly ISiteEventService _siteEventService;
        private readonly IHCSettingsService _settingsService;
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
            IHCMetricsStorage metricsStorage
        )
            : base()
        {
            _env = env;
            _eventDataSink = eventDataSink;
            _siteEventService = siteEventService;
            _settingsService = settingsService;
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
                    new FolderFileStorage("files_test", "Disk storage", @"C:\temp\fileStorageTest"),
                    new UrlFileStorage("urls_test", "External url")
                }
            }));
            UseModule(new HCAccessTokensModule(new HCAccessTokensModuleOptions()
            {
                TokenStorage = accessManagerTokenStorage
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
            UseModule(new HCAuditLogModule(new HCAuditLogModuleOptions() { AuditEventService = auditEventStorage }));
            UseModule(new HCSiteEventsModule(new HCSiteEventsModuleOptions() { SiteEventService = siteEventService }));
            UseModule(new HCSettingsModule(new HCSettingsModuleOptions() { Service = settingsService, ModelType = typeof(TestSettings) }));

            if (!_hasInited)
            {
                InitOnce();
            }
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
            //////////////

            config.ShowFailedModuleLoadStackTrace = new Maybe<RuntimeTestAccessRole>(RuntimeTestAccessRole.WebAdmins);
            config.PingAccess = new Maybe<RuntimeTestAccessRole>(RuntimeTestAccessRole.API);
            config.RedirectTargetOnNoAccess = "/no-access";
            config.IntegratedLoginConfig = new HCIntegratedLoginConfig
            {
                IntegratedLoginEndpoint = "/HCLogin/login",
                Current2FACodeExpirationTime = HCMfaTotpUtil.GetCurrentTotpCodeExpirationTime(),
                Send2FACodeEndpoint = "/hclogin/Request2FACode",
                TwoFactorCodeInputMode = HCIntegratedLoginConfig.HCLoginTwoFactorCodeInputMode.Optional,
                WebAuthnMode = HCIntegratedLoginConfig.HCLoginWebAuthnMode.Optional
            };

            var totpKey = "_dev_totp_secret";
            var webAuthnKey = "_dev_webAuthn_secret";
            config.IntegratedProfileConfig = new HCIntegratedProfileConfig
            {
                Username = CurrentRequestInformation.UserName,
                BodyHtml = "Here is some custom content.<ul><li><a href=\"https://www.google.com\">A link here</a></li></ul>",
                // TOTP: Elevate
                ShowTotpElevation = !string.IsNullOrWhiteSpace(Request.HttpContext.Session.GetString(totpKey))
                    && string.IsNullOrWhiteSpace(Request.HttpContext.Session.GetString("_dev_2fa_validated")),
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
                    var webAuthnResult = AsyncUtils.RunSync(() => webauthn.VerifyAssertion(options, d));
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

                    AsyncUtils.RunSync(() => webauthn.RegisterCredentials(options, attestation));
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
                },
            };
        }

        private static readonly DateTime _eventTime = DateTime.Now;
        protected override RequestInformation<RuntimeTestAccessRole> GetRequestInformation(HttpRequest request)
        {
            _eventDataSink.RegisterEvent("GetRequestInfo", new
            {
                Type = this.GetType().Name,
                Path = Request?.Path
            });
            HCMetricsContext.AddNote("Random value", new Random().Next());
            HCMetricsContext.AddGlobalValue("Rng", new Random().Next());

            if (ForcedRole != null)
            {
                return new RequestInformation<RuntimeTestAccessRole>(ForcedRole.Value, "forcedId", "Forced role user");
            }

            var roles = RuntimeTestAccessRole.Guest;

            if (request.Query.ContainsKey("siteEvents"))
            {
                for (int i = 0; i < 500; i++)
                {
                    HCSiteEventUtils.TryRegisterNewEvent(SiteEventSeverity.Warning, $"pageError_{_eventTime.Ticks}", $"Slow page #{_eventTime.Ticks}",
                        $"Pageload seems a bit slow currently on page #{_eventTime.Ticks}, we're working on it.",
                        duration: 5,
                        developerDetails: $"Duration: {i} ms");
                }
            }
            if (request.Query.ContainsKey("siteEvent"))
            {
                HCSiteEventUtils.TryRegisterNewEvent(SiteEventSeverity.Error, "api_x_error", "Oh no! API X is broken!", "How could this happen to us!?",
                    developerDetails: "Hmm this is probably why.",
                    config: x => x.AddRelatedLink("Status page", "https://status.otherapi.com"));
            }
            if (request.Query.ContainsKey("siteEventResolved"))
            {
                HCSiteEventUtils.TryRegisterResolvedEvent("api_x_error", "Seems it fixed itself somehow.",
                    config: x => x.AddRelatedLink("Another page", "https://www.google.com"));
            }
            if (request.Query.ContainsKey("simulateSiteEventResolveJob"))
            {
                SimulateSiteEventResolveJob();
            }

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
        private void SimulateSiteEventResolveJob()
        {
            var unresolvedEvents = HCSiteEventUtils.TryGetAllUnresolvedEvents();
            foreach (var unresolvedEvent in unresolvedEvents)
            {
                var timeSince = DateTimeOffset.Now - (unresolvedEvent.Timestamp + TimeSpan.FromMinutes(unresolvedEvent.Duration));
                if (timeSince > TimeSpan.FromMinutes(15))
                {
                    HCSiteEventUtils.TryRegisterResolvedEvent(unresolvedEvent.EventTypeId, "Seems to be fixed now.");
                }
            }
        }

        private HCWebAuthnHelper CreateWebAuthnHelper()
            => new HCWebAuthnHelper(new HCWebAuthnHelperOptions
            {
                ServerDomain = "localhost",
                ServerName = "HCDev",
                Origin = Request.Headers["Origin"]
            }, new HCMemoryWebAuthnCredentialManager());

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
            return new FileStreamResult(new FileStream(filepath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite), new MediaTypeHeaderValue("text/plain"))
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
                    SettingValue = _settingsService.GetSettings<TestSettings>().IntProp,
                    ExtraB = "BBBB"
                },
                2 => new
                {
                    Url = Request.GetDisplayUrl(),
                    User = CurrentRequestInformation?.UserName,
                    SettingValue = _settingsService.GetSettings<TestSettings>().IntProp,
                    ExtraA = "AAAA"
                },
                _ => new
                {
                    Url = Request.GetDisplayUrl(),
                    User = CurrentRequestInformation?.UserName,
                    SettingValue = _settingsService.GetSettings<TestSettings>().IntProp
                },
            };
            _eventDataSink.RegisterEvent("pageload", payload);
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

        private static bool _hasInited = false;
        private void InitOnce()
        {
            _hasInited = true;
            Task.Run(() => AddEvents());
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

            Config.IoCConfig.TestStreamA.InsertEntries(entriesToInsert);

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
