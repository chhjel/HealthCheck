#if NETFULL
using HealthCheck.Core.Abstractions.Modules;
using HealthCheck.Core.Attributes;
using HealthCheck.Core.Models;
using HealthCheck.Core.Modules.Tests.Attributes;
using HealthCheck.Core.Util;
using HealthCheck.Core.Util.Modules;
using HealthCheck.WebUI.Models;
using HealthCheck.WebUI.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.SessionState;

namespace HealthCheck.WebUI.Abstractions
{
    /// <summary>
    /// Base controller for the ui and api.
    /// </summary>
    /// <typeparam name="TAccessRole">Maybe{EnumType} used for access roles.</typeparam>
    [SessionState(SessionStateBehavior.ReadOnly)]
    public abstract class HealthCheckControllerBase<TAccessRole>: Controller
        where TAccessRole: Enum
    {
#region Properties & Fields
        /// <summary>
        /// Set to false to return 404 for all actions.
        /// <para>Enabled by default.</para>
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
        /// </summary>
        protected abstract RequestInformation<TAccessRole> GetRequestInformation(HttpRequestBase request);

        /// <summary>
        /// Configure access using the config parameter. Method is invoked from BeginExecute.
        /// </summary>
        protected abstract void ConfigureAccess(HttpRequestBase request, AccessConfig<TAccessRole> config);
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
        public TModule GetModule<TModule>() where TModule : class
            => Helper.GetModule<TModule>();
#endregion

#region Endpoints
        /// <summary>
        /// Returns the page html.
        /// </summary>
        [HideFromRequestLog]
        public virtual ActionResult Index()
        {
            if (!Enabled) return ThrowNotFound();
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
                    return ThrowNotFound();
                }
            }

            var frontEndOptions = GetFrontEndOptions();
            var pageOptions = GetPageOptions();
            var html = Helper.CreateViewHtml(CurrentRequestAccessRoles, frontEndOptions, pageOptions);
            return Content(html);
        }

        private ActionResult CreateIntegratedLoginViewResult()
        {
            var frontEndOptions = GetFrontEndOptions();
            frontEndOptions.ShowIntegratedLogin = true;
            frontEndOptions.IntegratedLoginEndpoint = Helper?.AccessConfig?.IntegratedLoginConfig?.IntegratedLoginEndpoint;
            frontEndOptions.IntegratedLoginShow2FA = Helper?.AccessConfig?.IntegratedLoginConfig?.Show2FAInput ?? false;
            frontEndOptions.IntegratedLoginSend2FACodeEndpoint = Helper?.AccessConfig?.IntegratedLoginConfig?.Send2FACodeEndpoint ?? "";
            frontEndOptions.IntegratedLoginSend2FACodeButtonText = Helper?.AccessConfig?.IntegratedLoginConfig?.Send2FACodeButtonText ?? "";
            frontEndOptions.IntegratedLoginCurrent2FACodeExpirationTime = Helper?.AccessConfig?.IntegratedLoginConfig?.Current2FACodeExpirationTime;
            frontEndOptions.IntegratedLogin2FACodeLifetime = Helper?.AccessConfig?.IntegratedLoginConfig?.TwoFactorCodeLifetime ?? 30;

            var pageOptions = GetPageOptions();
            var html = Helper.CreateViewHtml(CurrentRequestAccessRoles, frontEndOptions, pageOptions);
            return Content(html);
        }

        /// <summary>
        /// Invokes a module method.
        /// </summary>
        [HideFromRequestLog]
        [HttpPost]
        public async Task<ActionResult> InvokeModuleMethod(string moduleId, string methodName, string jsonPayload)
        {
            if (!Enabled) return ThrowNotFound();

            var result = await Helper.InvokeModuleMethod(CurrentRequestInformation, moduleId, methodName, jsonPayload);
            if (!result.HasAccess)
            {
                return ThrowNotFound();
            }
            return Content(result.Result);
        }

        /// <summary>
        /// Used for any dynamic actions from registered modules.
        /// </summary>
        [HideFromRequestLog]
        protected override void HandleUnknownAction(string actionName)
        {
            ActionResult CreateContentResult(string content) => Content(content);
            FileResult CreateFileResult(Stream stream, string filename) => File(stream, "content-disposition", filename);

            var url = Request.RawUrl.ToString();
            url = url.Substring(url.ToLower().Trim().IndexOf($"/{actionName.ToLower()}"));

            var content = Helper.InvokeModuleAction(CurrentRequestInformation, actionName, url).Result;
            if (content?.HasAccess == true && content.Result != null)
            {
                var result = content.Result;
                if (result is string contentStr)
                {
                    CreateContentResult(contentStr).ExecuteResult(this.ControllerContext);
                }
                else if (result is HealthCheckFileStreamResult stream)
                {
                    var filename = stream.FileName;
                    CreateFileResult(stream.ContentStream, filename).ExecuteResult(this.ControllerContext);
                }
                return;
            }

            base.HandleUnknownAction(actionName);
        }

        /// <summary>
        /// Returns 'OK' and 200 status code.
        /// </summary>
        [HideFromRequestLog]
        public virtual ActionResult Ping()
        {
            if (!Enabled || !Helper.CanUsePingEndpoint(CurrentRequestAccessRoles))
                return ThrowNotFound();

            return Content("OK");
        }
#endregion

#region Virtuals
        /// <summary>
        /// Resolve client ip from the current request.
        /// </summary>
        protected virtual string GetRequestIP(RequestContext requestContext)
            => RequestUtils.GetIPAddress(requestContext?.HttpContext?.Request);
#endregion

#region Overrides
        /// <summary>
        /// Calls GetRequestAccessRoles and SetOptions.
        /// </summary>
        protected override IAsyncResult BeginExecute(RequestContext requestContext, AsyncCallback callback, object state)
        {
            var request = requestContext?.HttpContext?.Request;

            CurrentRequestInformation = GetRequestInformation(request);
            CurrentRequestInformation.Method = request?.HttpMethod;
            CurrentRequestInformation.Url = request?.Url?.ToString();
            CurrentRequestInformation.ClientIP = GetRequestIP(requestContext);
            CurrentRequestInformation.Headers = request?.Headers?.AllKeys?.ToDictionary(t => t, t => request.Headers[t])
                ?? new Dictionary<string, string>();

            var requestInfoOverridden = Helper.ApplyTokenAccessIfDetected(CurrentRequestInformation);
            CurrentRequestAccessRoles = CurrentRequestInformation?.AccessRole;

            if (!requestInfoOverridden)
            {
                ConfigureAccess(request, Helper.AccessConfig);
            }
            Helper.AfterConfigure(CurrentRequestInformation);
            return base.BeginExecute(requestContext, callback, state);
        }
#endregion

#region Helpers
        /// <summary>
        /// Serializes the given object into a json result.
        /// </summary>
        protected ActionResult CreateJsonResult(object obj)
            => Content(Helper.SerializeJson(obj), "application/json");

        /// <summary>
        /// Throws a <see cref="HttpException"/>.
        /// </summary>
        protected ActionResult ThrowNotFound() => throw new HttpException(404, "Not Found");
#endregion
    }
}
#endif
