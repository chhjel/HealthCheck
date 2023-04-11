#if NETCORE
using QoDL.Toolkit.Core.Abstractions.Modules;
using QoDL.Toolkit.Core.Attributes;
using QoDL.Toolkit.Core.Config;
using QoDL.Toolkit.Core.Extensions;
using QoDL.Toolkit.Core.Models;
using QoDL.Toolkit.Core.Modules.Tests.Attributes;
using QoDL.Toolkit.Core.Util;
using QoDL.Toolkit.Core.Util.Modules;
using QoDL.Toolkit.Web.Core.Utils;
using QoDL.Toolkit.WebUI.Models;
using QoDL.Toolkit.WebUI.Util;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace QoDL.Toolkit.WebUI.Abstractions
{
    /// <summary>
    /// Base controller for the ui and api.
    /// </summary>
    /// <typeparam name="TAccessRole">Maybe{EnumType} used for access roles.</typeparam>
    [Route("[controller]")]
    public abstract class ToolkitControllerBase<TAccessRole> : Controller
        where TAccessRole : Enum
    {
#region Properties & Fields
        /// <summary>
        /// Set to false to return 404 for all actions.
        /// <para>True by default.</para>
        /// </summary>
        protected bool Enabled { get; set; } = true;

        /// <summary>
        /// Access roles for the current request. Is only set after BeginExecute has been called for the request.
        /// <para>Value equals what you return from GetRequestInformation().AccessRole.</para>
        /// </summary>
        protected Maybe<TAccessRole> CurrentRequestAccessRoles { get; set; }

        /// <summary>
        /// Information about the current request. Is only set after BeginExecute has been called for the request.
        /// <para>Value equals what you return from GetRequestInformation.</para>
        /// </summary>
        protected RequestInformation<TAccessRole> CurrentRequestInformation { get; set; }

        private readonly ToolkitControllerHelper<TAccessRole> Helper;
#endregion

        /// <summary>
        /// Base controller for the ui and api.
        /// </summary>
        protected ToolkitControllerBase()
        {
            Helper = new ToolkitControllerHelper<TAccessRole>(() => GetPageOptions(), () => GetFrontEndOptions());
        }

#region Abstract
        /// <summary>
        /// Get front-end options.
        /// </summary>
        protected abstract TKFrontEndOptions GetFrontEndOptions();

        /// <summary>
        /// Get page options.
        /// </summary>
        protected abstract TKPageOptions GetPageOptions();

        /// <summary>
        /// Should return a custom enum flag object with the roles of the current user. Must match the type used in <see cref="RuntimeTestAttribute.RolesWithAccess"/>.
        /// <para>Returns null by default to allow all test.</para>
        /// </summary>
        protected abstract RequestInformation<TAccessRole> GetRequestInformation(HttpRequest request);

        /// <summary>
        /// Set any options here. Method is invoked from BeginExecute.
        /// </summary>
        protected abstract void ConfigureAccess(HttpRequest request, AccessConfig<TAccessRole> options);
#endregion

#region Modules
        /// <summary>
        /// Register a module that will be available.
        /// </summary>
        protected TModule UseModule<TModule>(TModule module, string name = null)
            where TModule: IToolkitModule
            => Helper.UseModule(module, name);

        /// <summary>
        /// Get the first registered module of the given type.
        /// </summary>
        protected TModule GetModule<TModule>() where TModule : class
            => Helper.GetModule<TModule>();
#endregion

#region Endpoints
        /// <summary>
        /// Returns the page html.
        /// </summary>
        [HideFromRequestLog]
        public virtual ActionResult Index()
        {
            if (!Enabled) return NotFound();
            else if (!Helper.HasAccessToAnyContent(CurrentRequestAccessRoles))
            {
                if (Helper.AccessConfig.UseIntegratedLogin)
                {
                    return CreateIntegratedLoginViewResult();
                }

                var queryStringState = "h=" + System.Web.HttpUtility.UrlEncode($"{Request.Query?["h"].FirstOrDefault() ?? ""}");
                var redirectTarget = Helper.AccessConfig.RedirectTargetOnNoAccessUsingRequest?.Invoke(Request, queryStringState);
                if (!string.IsNullOrWhiteSpace(redirectTarget))
                {
                    return Redirect(redirectTarget);
                }
                else if (!string.IsNullOrWhiteSpace(Helper.AccessConfig.RedirectTargetOnNoAccess))
                {
                    return Redirect(Helper.AccessConfig.RedirectTargetOnNoAccess);
                }
                else
                {
                    return NotFound();
                }
            }

            var frontEndOptions = GetFrontEndOptions();
            frontEndOptions.EditorConfig.SetDefaults(frontEndOptions.EndpointBase);
            frontEndOptions.IntegratedProfileConfig = Helper?.AccessConfig?.IntegratedProfileConfig ?? new TKIntegratedProfileConfig();
            if (frontEndOptions?.IntegratedProfileConfig?.Hide == false)
            {
                frontEndOptions.UserRoles = EnumUtils.TryGetEnumFlaggedValueNames(CurrentRequestAccessRoles.Value);
                if (frontEndOptions.IntegratedProfileConfig.ShowToolkitCategories)
                {
                    frontEndOptions.UserModuleCategories = Helper.GetUserModuleCategories(CurrentRequestAccessRoles);
                }
            }
            frontEndOptions.AllowAccessTokenKillswitch = CurrentRequestInformation.AllowAccessTokenKillswitch;

            var pageOptions = GetPageOptions();
            var html = Helper.CreateViewHtml(CurrentRequestAccessRoles, frontEndOptions, pageOptions);

            return new ContentResult()
            {
                Content = html,
                ContentType = "text/html",
            };
        }

        private ActionResult CreateIntegratedLoginViewResult()
        {
            var frontEndOptions = GetFrontEndOptions();
            frontEndOptions.EditorConfig.SetDefaults(frontEndOptions.EndpointBase);
            frontEndOptions.ShowIntegratedLogin = true;
            frontEndOptions.IntegratedLoginEndpoint = Helper?.AccessConfig?.IntegratedLoginConfig?.IntegratedLoginEndpoint;
            frontEndOptions.IntegratedLoginSend2FACodeEndpoint = Helper?.AccessConfig?.IntegratedLoginConfig?.TwoFactorCodeConfig?.Send2FACodeEndpoint ?? "";
            frontEndOptions.IntegratedLoginSend2FACodeButtonText = Helper?.AccessConfig?.IntegratedLoginConfig?.TwoFactorCodeConfig?.Send2FACodeButtonText ?? "";
            frontEndOptions.IntegratedLoginCurrent2FACodeExpirationTime = Helper?.AccessConfig?.IntegratedLoginConfig?.TwoFactorCodeConfig?.Current2FACodeExpirationTime;
            frontEndOptions.IntegratedLogin2FACodeLifetime = Helper?.AccessConfig?.IntegratedLoginConfig?.TwoFactorCodeConfig?.TwoFactorCodeLifetime ?? 30;
            frontEndOptions.IntegratedLoginTwoFactorCodeInputMode = Helper?.AccessConfig?.IntegratedLoginConfig?.TwoFactorCodeConfig?.TwoFactorCodeInputMode ?? TKIntegratedLoginConfig.TKLoginTwoFactorCodeInputMode.Off;
            frontEndOptions.IntegratedLoginWebAuthnMode = Helper?.AccessConfig?.IntegratedLoginConfig?.WebAuthnConfig?.WebAuthnMode ?? TKIntegratedLoginConfig.TKLoginWebAuthnMode.Off;
            frontEndOptions.IntegratedLogin2FANote = Helper?.AccessConfig?.IntegratedLoginConfig?.TwoFactorCodeConfig?.Note;
            frontEndOptions.IntegratedLoginWebAuthnNote= Helper?.AccessConfig?.IntegratedLoginConfig?.WebAuthnConfig?.Note;

            var pageOptions = GetPageOptions();
            var html = Helper.CreateViewHtml(CurrentRequestAccessRoles, frontEndOptions, pageOptions);

            return new ContentResult()
            {
                Content = html,
                ContentType = "text/html",
            };
        }

        /// <summary>
        /// Invokes a module method.
        /// </summary>
        [HideFromRequestLog]
        [HttpPost]
        [Route("InvokeModuleMethod")]
        public async Task<ActionResult> InvokeModuleMethod([FromBody] InvokeModuleMethodRequest model)
        {
            if (!Enabled) return NotFound();

            var result = await Helper.InvokeModuleMethod(CurrentRequestInformation, model.moduleId, model.methodName, model.jsonPayload, model.isB64);
            if (!result.HasAccess)
            {
                return NotFound();
            }
            return Content(result.Result, "application/json");
        }

        /// <summary>
        /// Used for any dynamic actions from registered modules.
        /// </summary>
        [HideFromRequestLog]
        [HttpHead]
        [Route("{actionName}/{subAction?}")]
        public IActionResult HandleUnknownAction_HEAD([FromRoute] string actionName) => HandleUnknownAction_GET(actionName);

        /// <summary>
        /// Used for any dynamic actions from registered modules.
        /// </summary>
        [HideFromRequestLog]
        [HttpPost]
        [Route("{actionName}/{subAction?}")]
        public IActionResult HandleUnknownAction_POST([FromRoute] string actionName) => HandleUnknownAction_GET(actionName);

        /// <summary>
        /// Used for any dynamic actions from registered modules.
        /// </summary>
        [HideFromRequestLog]
        [HttpGet]
        [Route("{actionName}/{subAction?}")]
        public IActionResult HandleUnknownAction_GET([FromRoute] string actionName)
        {
            ActionResult CreateContentResult(string content) => Content(content, "text/html");

            var url = Request.GetDisplayUrl();
            url = url[url.ToLower().Trim().IndexOf($"/{actionName.ToLower()}")..];

            var content = Helper.InvokeModuleAction(CurrentRequestInformation, actionName, url).Result;
            if (content?.HasAccess == true && content.Result != null)
            {
                var result = content.Result;
                if (result is string contentStr)
                {
                    return CreateContentResult(contentStr);
                }
                else if (result is ToolkitStatusCodeOnlyResult statusCodeResult)
                {
                    return StatusCode((int)statusCodeResult.Code);
                }
                else if (result is ToolkitFileDownloadResult file)
                {
                    // Cookies
                    foreach (var cookie in file.CookiesToDelete ?? Enumerable.Empty<string>())
                    {
                        Response.Cookies.Delete(cookie);
                    }
                    foreach (var cookie in file.CookiesToSet ?? new())
                    {
                        Response.Cookies.Append(cookie.Key, cookie.Value);
                    }

                    // Contents
                    if (file.Stream != null)
                    {
                        return File(file.Stream, file.ContentType, file.FileName);
                    }
                    else if (file.Bytes != null)
                    {
                        return File(file.Bytes, file.ContentType, file.FileName);
                    }
                    else
                    {
                        var encoding = file.Encoding ?? Encoding.UTF8;
                        return File(encoding.GetBytes(file.Content ?? ""), file.ContentType, file.FileName);
                    }
                }
            }

            return NotFound();
        }

        /// <summary>
        /// Returns 'OK' and 200 status code.
        /// </summary>
        [HideFromRequestLog]
        [Route("Ping")]
        public virtual ActionResult Ping()
        {
            if (!Enabled || !Helper.CanUsePingEndpoint(CurrentRequestAccessRoles))
                return NotFound();

            return Content("OK");
        }

        #region Resources
        /// <summary>
        /// Loads assets from memory if <c>QoDL.Toolkit.WebUI.Assets</c> is used.
        /// </summary>
        [HideFromRequestLog]
        [Route("GetAsset")]
        public virtual ActionResult GetAsset(string n, string v = null)
        {
            Response.Headers["Cache-Control"] = "private, max-age=3600";
            if (TKAssetGlobalConfig.AssetCache.TryGetValue(n, out var content))
            {
                var contentType = Helper.GetAssetContentType(n);
                return Content(content, contentType);
            }
            else if (TKAssetGlobalConfig.BinaryAssetCache.TryGetValue(n, out var contentBytes))
            {
                var contentType = Helper.GetAssetContentType(n);
                return File(contentBytes, contentType);
            }
            return NotFound();
        }
        #endregion

        #region MFA
        /// <summary>
        /// Attempts to elevate access using a TOTP code if enabled.
        /// </summary>
        [HideFromRequestLog]
        [HttpPost]
        [Route("ProfileElevateTotp")]
        public virtual async Task<ActionResult> ProfileElevateTotp([FromBody] TKProfileElevateTotpRequest model)
        {
            if (!Enabled
                || Helper?.AccessConfig?.IntegratedProfileConfig?.TotpElevationEnabled != true
                || Helper?.HasAccessToAnyContent(CurrentRequestAccessRoles) != true)
                return NotFound();

            var result = await Helper.AccessConfig.IntegratedProfileConfig.TotpElevationLogic(model?.Code);
            return CreateJsonResult(result);
        }

        /// <summary>
        /// Attempts to register a TOTP code if enabled.
        /// </summary>
        [HideFromRequestLog]
        [HttpPost]
        [Route("ProfileRegisterTotp")]
        public virtual async Task<ActionResult> ProfileRegisterTotp([FromBody] TKProfileRegisterTotpRequest model)
        {
            if (!Enabled
                || Helper?.AccessConfig?.IntegratedProfileConfig?.AddTotpEnabled != true
                || Helper?.HasAccessToAnyContent(CurrentRequestAccessRoles) != true)
                return NotFound();

            var result = await Helper.AccessConfig.IntegratedProfileConfig.AddTotpLogic(model?.Password, model?.Secret, model?.Code);
            return CreateJsonResult(result);
        }

        /// <summary>
        /// Attempts to remove any TOTP code from the profile if enabled.
        /// </summary>
        [HideFromRequestLog]
        [HttpPost]
        [Route("ProfileRemoveTotp")]
        public virtual async Task<ActionResult> ProfileRemoveTotp([FromBody] TKProfileRemoveTotpRequest model)
        {
            if (!Enabled
                || Helper?.AccessConfig?.IntegratedProfileConfig?.RemoveTotpEnabled != true
                || Helper?.HasAccessToAnyContent(CurrentRequestAccessRoles) != true)
                return NotFound();

            var result = await Helper.AccessConfig.IntegratedProfileConfig.RemoveTotpLogic(model?.Password);
            return CreateJsonResult(result);
        }

        /// <summary>
        /// Attempts to create assertion options if enabled.
        /// </summary>
        [HideFromRequestLog]
        [HttpPost]
        [Route("ProfileCreateWebAuthnAssertionOptions")]
        public virtual async Task<ActionResult> ProfileCreateWebAuthnAssertionOptions([FromBody] TKCreateWebAuthnAssertionOptionsRequest model)
        {
            if (!Enabled
                || Helper?.AccessConfig?.IntegratedProfileConfig?.WebAuthnElevationEnabled != true
                || Helper?.HasAccessToAnyContent(CurrentRequestAccessRoles) != true)
                return NotFound();

            var options = await Helper.AccessConfig.IntegratedProfileConfig.CreateWebAuthnAssertionOptionsLogic(model?.UserName);
            return CreateJsonResult(options, stringEnums: false);
        }

        /// <summary>
        /// Attempts to elevate access using WebAuthn if enabled.
        /// </summary>
        [HideFromRequestLog]
        [HttpPost]
        [Route("ProfileElevateWebAuthn")]
        public virtual async Task<ActionResult> ProfileElevateWebAuthn([FromBody] TKProfileElevateWebAuthnRequest model)
        {
            if (!Enabled
                || Helper?.AccessConfig?.IntegratedProfileConfig?.WebAuthnElevationEnabled != true
                || Helper?.HasAccessToAnyContent(CurrentRequestAccessRoles) != true)
                return NotFound();

            var result = await Helper.AccessConfig.IntegratedProfileConfig.WebAuthnElevationLogic(model?.Data);
            return CreateJsonResult(result);
        }

        /// <summary>
        /// Attempts to create WebAuthn register options if enabled.
        /// </summary>
        [HideFromRequestLog]
        [HttpPost]
        [Route("ProfileCreateWebAuthnRegistrationOptions")]
        public virtual async Task<ActionResult> ProfileCreateWebAuthnRegistrationOptions([FromBody] TKCreateWebAuthnRegistrationOptionsRequest model)
        {
            if (!Enabled
                || Helper?.AccessConfig?.IntegratedProfileConfig?.AddWebAuthnEnabled != true
                || Helper?.HasAccessToAnyContent(CurrentRequestAccessRoles) != true)
                return NotFound();

            var options = await Helper.AccessConfig.IntegratedProfileConfig.CreateWebAuthnRegistrationOptionsLogic(model?.UserName, model?.Password);
            return CreateJsonResult(options, stringEnums: false);
        }

        /// <summary>
        /// Attempts to register WebAuthn if enabled.
        /// </summary>
        [HideFromRequestLog]
        [HttpPost]
        [Route("ProfileRegisterWebAuthn")]
        public virtual async Task<ActionResult> ProfileRegisterWebAuthn([FromBody] TKProfileRegisterWebAuthnRequest model)
        {
            if (!Enabled
                || Helper?.AccessConfig?.IntegratedProfileConfig?.AddWebAuthnEnabled != true
                || Helper?.HasAccessToAnyContent(CurrentRequestAccessRoles) != true)
                return NotFound();

            var result = await Helper.AccessConfig.IntegratedProfileConfig.AddWebAuthnLogic(model?.Password, model?.RegistrationData);
            return CreateJsonResult(result);
        }

        /// <summary>
        /// Attempts to remove WebAuthn from the profile if enabled.
        /// </summary>
        [HideFromRequestLog]
        [HttpPost]
        [Route("ProfileRemoveWebAuthn")]
        public virtual async Task<ActionResult> ProfileRemoveWebAuthn([FromBody] TKProfileRemoveWebAuthnRequest model)
        {
            if (!Enabled
                || Helper?.AccessConfig?.IntegratedProfileConfig?.RemoveWebAuthnEnabled != true
                || Helper?.HasAccessToAnyContent(CurrentRequestAccessRoles) != true)
                return NotFound();

            var result = await Helper.AccessConfig.IntegratedProfileConfig.RemoveWebAuthnLogic(model?.Password);
            return CreateJsonResult(result);
        }
#endregion
#endregion

#region Virtuals
        /// <summary>
        /// Resolve client ip from the current request.
        /// </summary>
        protected virtual string GetRequestIP(ActionExecutingContext context)
            => TKRequestUtils.GetIPAddress(context?.HttpContext);
#endregion

#region Overrides
        /// <summary>
        /// Calls GetRequestInformation and SetOptions.
        /// </summary>
        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var request = context?.HttpContext?.Request;
            var url = request?.GetDisplayUrl();
            if (ToolkitControllerHelper<TAccessRole>.ShouldEnableRequestBuffering())
            {
                request?.EnableBuffering();
            }

            CurrentRequestInformation = GetRequestInformation(request);
            CurrentRequestInformation.Method = request?.Method;
            CurrentRequestInformation.Url = url;
            CurrentRequestInformation.ClientIP = GetRequestIP(context);
            CurrentRequestInformation.Headers = request?.Headers.Keys?.ToDictionaryIgnoreDuplicates(t => t, t => request.Headers[t].ToString())
                ?? new Dictionary<string, string>();
            CurrentRequestInformation.InputStream = request.Body;
            if (request.HasFormContentType && request?.Form?.Files?.Any() == true)
            {
                foreach(var file in request.Form.Files)
                {
                    CurrentRequestInformation.FormFiles.Add(new RequestFormFile
                    {
                        FileName = file.FileName,
                        Length = file.Length,
                        GetStream = () => file.OpenReadStream()
                    });
                }
            }
            
            var requestInfoOverridden = Helper.ApplyTokenAccessIfDetected(CurrentRequestInformation);
            CurrentRequestAccessRoles = CurrentRequestInformation?.AccessRole;

            if (!requestInfoOverridden)
            {
                ConfigureAccess(request, Helper.AccessConfig);
            }
            Helper.AfterConfigure(CurrentRequestInformation);
            await base.OnActionExecutionAsync(context, next);
        }
#endregion

#region Helpers
        /// <summary>
        /// Serializes the given object into a json result.
        /// </summary>
        protected ActionResult CreateJsonResult(object obj, bool stringEnums = true)
            => Content(ToolkitControllerHelper<TAccessRole>.SerializeJson(obj, stringEnums), "application/json");

        /// <summary>
        /// Request model sent to <see cref="InvokeModuleMethod"/>.
        /// </summary>
        public class InvokeModuleMethodRequest
        {
#pragma warning disable IDE1006 // Naming Styles
            /// <summary>Id of the module to invoke.</summary>
            public string moduleId { get; set; }

            /// <summary>Name of the method to invoke.</summary>
            public string methodName { get; set; }

            /// <summary>Data to passto invoked method.</summary>
            public string jsonPayload { get; set; }

            /// <summary>True if <see cref="jsonPayload"/> is in b64 format.</summary>
            public bool isB64 { get; set; }
#pragma warning restore IDE1006 // Naming Styles
        }
#endregion
    }
}
#endif
