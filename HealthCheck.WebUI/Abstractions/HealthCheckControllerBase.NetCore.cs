#if NETCORE
using HealthCheck.Core.Abstractions.Modules;
using HealthCheck.Core.Attributes;
using HealthCheck.Core.Extensions;
using HealthCheck.Core.Models;
using HealthCheck.Core.Modules.Tests.Attributes;
using HealthCheck.Core.Util;
using HealthCheck.Core.Util.Modules;
using HealthCheck.WebUI.Models;
using HealthCheck.WebUI.Util;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace HealthCheck.WebUI.Abstractions
{
    /// <summary>
    /// Base controller for the ui and api.
    /// </summary>
    /// <typeparam name="TAccessRole">Maybe{EnumType} used for access roles.</typeparam>
    [Route("[controller]")]
    public abstract class HealthCheckControllerBase<TAccessRole> : Controller
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

        private readonly HealthCheckControllerHelper<TAccessRole> Helper;
#endregion

        /// <summary>
        /// Base controller for the ui and api.
        /// </summary>
        protected HealthCheckControllerBase()
        {
            Helper = new HealthCheckControllerHelper<TAccessRole>(() => GetPageOptions());
        }

#region Abstract
        /// <summary>
        /// Get front-end options.
        /// </summary>
        protected abstract HCFrontEndOptions GetFrontEndOptions();

        /// <summary>
        /// Get page options.
        /// </summary>
        protected abstract HCPageOptions GetPageOptions();

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
            where TModule: IHealthCheckModule
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

                var redirectTarget = Helper.AccessConfig.RedirectTargetOnNoAccessUsingRequest?.Invoke(Request);
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
            frontEndOptions.IntegratedProfileConfig = Helper?.AccessConfig?.IntegratedProfileConfig ?? new HCIntegratedProfileConfig();
            if (frontEndOptions?.IntegratedProfileConfig?.Hide == false)
            {
                frontEndOptions.UserRoles = EnumUtils.TryGetEnumFlaggedValueNames(CurrentRequestAccessRoles.Value);
            }

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
            frontEndOptions.ShowIntegratedLogin = true;
            frontEndOptions.IntegratedLoginEndpoint = Helper?.AccessConfig?.IntegratedLoginConfig?.IntegratedLoginEndpoint;
            frontEndOptions.IntegratedLoginSend2FACodeEndpoint = Helper?.AccessConfig?.IntegratedLoginConfig?.Send2FACodeEndpoint ?? "";
            frontEndOptions.IntegratedLoginSend2FACodeButtonText = Helper?.AccessConfig?.IntegratedLoginConfig?.Send2FACodeButtonText ?? "";
            frontEndOptions.IntegratedLoginCurrent2FACodeExpirationTime = Helper?.AccessConfig?.IntegratedLoginConfig?.Current2FACodeExpirationTime;
            frontEndOptions.IntegratedLogin2FACodeLifetime = Helper?.AccessConfig?.IntegratedLoginConfig?.TwoFactorCodeLifetime ?? 30;
            frontEndOptions.IntegratedLoginTwoFactorCodeInputMode = Helper?.AccessConfig?.IntegratedLoginConfig?.TwoFactorCodeInputMode ?? HCIntegratedLoginConfig.HCLoginTwoFactorCodeInputMode.Off;
            frontEndOptions.IntegratedLoginWebAuthnMode = Helper?.AccessConfig?.IntegratedLoginConfig?.WebAuthnMode ?? HCIntegratedLoginConfig.HCLoginWebAuthnMode.Off;

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

            var result = await Helper.InvokeModuleMethod(CurrentRequestInformation, model.moduleId, model.methodName, model.jsonPayload);
            if (!result.HasAccess)
            {
                return NotFound();
            }
            return Content(result.Result);
        }

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
            FileResult CreateFileResult(Stream stream, string filename) => File(stream, "text/plain", filename);

            var url = Request.GetDisplayUrl();
            url = url.Substring(url.ToLower().Trim().IndexOf($"/{actionName.ToLower()}"));

            var content = Helper.InvokeModuleAction(CurrentRequestInformation, actionName, url).Result;
            if (content?.HasAccess == true && content.Result != null)
            {
                var result = content.Result;
                if (result is string contentStr)
                {
                    return CreateContentResult(contentStr);
                }
                else if (result is HealthCheckFileStreamResult stream)
                {
                    var filename = stream.FileName;
                    return CreateFileResult(stream.ContentStream, filename);
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

#region MFA
        /// <summary>
        /// Attempts to elevate access using a TOTP code if enabled.
        /// </summary>
        [HideFromRequestLog]
        [HttpPost]
        [Route("ProfileElevateTotp")]
        public virtual ActionResult ProfileElevateTotp([FromBody] HCProfileElevateTotpRequest model)
        {
            if (!Enabled
                || Helper?.AccessConfig?.IntegratedProfileConfig?.TotpElevationEnabled != true
                || Helper?.HasAccessToAnyContent(CurrentRequestAccessRoles) != true)
                return NotFound();

            var result = Helper.AccessConfig.IntegratedProfileConfig.TotpElevationLogic(model?.Code);
            return Json(result);
        }

        /// <summary>
        /// Attempts to register a TOTP code if enabled.
        /// </summary>
        [HideFromRequestLog]
        [HttpPost]
        [Route("ProfileRegisterTotp")]
        public virtual ActionResult ProfileRegisterTotp([FromBody] HCProfileRegisterTotpRequest model)
        {
            if (!Enabled
                || Helper?.AccessConfig?.IntegratedProfileConfig?.AddTotpEnabled != true
                || Helper?.HasAccessToAnyContent(CurrentRequestAccessRoles) != true)
                return NotFound();

            var result = Helper.AccessConfig.IntegratedProfileConfig.AddTotpLogic(model?.Password, model?.Secret, model?.Code);
            return Json(result);
        }

        /// <summary>
        /// Attempts to remove any TOTP code from the profile if enabled.
        /// </summary>
        [HideFromRequestLog]
        [HttpPost]
        [Route("ProfileRemoveTotp")]
        public virtual ActionResult ProfileRemoveTotp([FromBody] HCProfileRemoveTotpRequest model)
        {
            if (!Enabled
                || Helper?.AccessConfig?.IntegratedProfileConfig?.RemoveTotpEnabled != true
                || Helper?.HasAccessToAnyContent(CurrentRequestAccessRoles) != true)
                return NotFound();

            var result = Helper.AccessConfig.IntegratedProfileConfig.RemoveTotpLogic(model?.Password);
            return Json(result);
        }

        /// <summary>
        /// Attempts to elevate access using WebAuthn if enabled.
        /// </summary>
        [HideFromRequestLog]
        [HttpPost]
        [Route("ProfileElevateWebAuthn")]
        public virtual ActionResult ProfileElevateWebAuthn([FromBody] HCProfileElevateWebAuthnRequest model)
        {
            if (!Enabled
                || Helper?.AccessConfig?.IntegratedProfileConfig?.WebAuthnElevationEnabled != true
                || Helper?.HasAccessToAnyContent(CurrentRequestAccessRoles) != true)
                return NotFound();

            var result = Helper.AccessConfig.IntegratedProfileConfig.WebAuthnElevationLogic(model?.Data);
            return Json(result);
        }

        /// <summary>
        /// Attempts to create register options WebAuthn if enabled.
        /// </summary>
        [HideFromRequestLog]
        [HttpPost]
        [Route("ProfileCreateWebAuthnRegistrationOptions")]
        public virtual ActionResult ProfileCreateWebAuthnRegistrationOptions([FromBody] HCCreateWebAuthnRegistrationOptionsRequest model)
        {
            if (!Enabled
                || Helper?.AccessConfig?.IntegratedProfileConfig?.AddWebAuthnEnabled != true
                || Helper?.HasAccessToAnyContent(CurrentRequestAccessRoles) != true)
                return NotFound();

            var options = Helper.AccessConfig.IntegratedProfileConfig.CreateWebAuthnRegistrationOptionsLogic(model?.UserName, model?.Password);
            return CreateJsonResult(options, stringEnums: false);
        }

        /// <summary>
        /// Attempts to register WebAuthn if enabled.
        /// </summary>
        [HideFromRequestLog]
        [HttpPost]
        [Route("ProfileRegisterWebAuthn")]
        public virtual ActionResult ProfileRegisterWebAuthn([FromBody] HCProfileRegisterWebAuthnRequest model)
        {
            if (!Enabled
                || Helper?.AccessConfig?.IntegratedProfileConfig?.AddWebAuthnEnabled != true
                || Helper?.HasAccessToAnyContent(CurrentRequestAccessRoles) != true)
                return NotFound();

            var result = Helper.AccessConfig.IntegratedProfileConfig.AddWebAuthnLogic(model?.Password, model?.RegistrationData);
            return Json(result);
        }

        /// <summary>
        /// Attempts to remove WebAuthn from the profile if enabled.
        /// </summary>
        [HideFromRequestLog]
        [HttpPost]
        [Route("ProfileRemoveWebAuthn")]
        public virtual ActionResult ProfileRemoveWebAuthn([FromBody] HCProfileRemoveWebAuthnRequest model)
        {
            if (!Enabled
                || Helper?.AccessConfig?.IntegratedProfileConfig?.RemoveWebAuthnEnabled != true
                || Helper?.HasAccessToAnyContent(CurrentRequestAccessRoles) != true)
                return NotFound();

            var result = Helper.AccessConfig.IntegratedProfileConfig.RemoveWebAuthnLogic(model?.Password);
            return Json(result);
        }
#endregion
#endregion

#region Virtuals
        /// <summary>
        /// Resolve client ip from the current request.
        /// </summary>
        protected virtual string GetRequestIP(ActionExecutingContext context)
            => RequestUtils.GetIPAddress(context?.HttpContext);
#endregion

#region Overrides
        /// <summary>
        /// Calls GetRequestInformation and SetOptions.
        /// </summar
        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var request = context?.HttpContext?.Request;

            CurrentRequestInformation = GetRequestInformation(request);
            CurrentRequestInformation.Method = request?.Method;
            CurrentRequestInformation.Url = request?.GetDisplayUrl();
            CurrentRequestInformation.ClientIP = GetRequestIP(context);
            CurrentRequestInformation.Headers = request?.Headers.Keys?.ToDictionaryIgnoreDuplicates(t => t, t => request.Headers[t].ToString())
                ?? new Dictionary<string, string>();
            
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
            => Content(Helper.SerializeJson(obj, stringEnums), "application/json");

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
#pragma warning restore IDE1006 // Naming Styles
        }
#endregion
    }
}
#endif
