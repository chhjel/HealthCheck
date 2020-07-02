#if NETCORE
using HealthCheck.Core.Abstractions.Modules;
using HealthCheck.Core.Attributes;
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
    public abstract class HealthCheckControllerBase<TAccessRole> : Controller
        where TAccessRole : Enum
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
            if (!Enabled) return NotFound();
            else if (!Helper.HasAccessToAnyContent(CurrentRequestAccessRoles))
            {
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
        public async Task<ActionResult> InvokeModuleMethod(string moduleId, string methodName, string jsonPayload)
        {
            if (!Enabled) return NotFound();

            var result = await Helper.InvokeModuleMethod(CurrentRequestInformation, moduleId, methodName, jsonPayload);
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
        [HttpGet("{actionName}")]
        public IActionResult HandleUnknownAction(string actionName)
        {
            ActionResult CreateContentResult(string content) => Content(content);
            FileResult CreateFileResult(Stream stream, string filename) => File(stream, "content-disposition", filename);

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
                    CreateFileResult(stream.ContentStream, filename).ExecuteResult(this.ControllerContext);
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
        /// </summary>
        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var request = context?.HttpContext?.Request;

            CurrentRequestInformation = GetRequestInformation(request);
            CurrentRequestInformation.Method = request?.Method;
            CurrentRequestInformation.Url = request?.GetDisplayUrl();
            CurrentRequestInformation.ClientIP = GetRequestIP(context);
            CurrentRequestInformation.Headers = request?.Headers.Keys?.ToDictionary(t => t, t => request.Headers[t].ToString())
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
        protected ActionResult CreateJsonResult(object obj)
            => Content(Helper.SerializeJson(obj), "application/json");
#endregion
    }
}
#endif
