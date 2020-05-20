#if NETFULL
using HealthCheck.Core.Abstractions;
using HealthCheck.Core.Abstractions.Modules;
using HealthCheck.Core.Attributes;
using HealthCheck.Core.Entities;
using HealthCheck.Core.Enums;
using HealthCheck.Core.Modules.Dataflow;
using HealthCheck.Core.Modules.EventNotifications;
using HealthCheck.Core.Modules.LogViewer.Models;
using HealthCheck.Core.Modules.Tests.Attributes;
using HealthCheck.Core.Services;
using HealthCheck.Core.Util;
using HealthCheck.WebUI;
using HealthCheck.WebUI.Models;
using HealthCheck.WebUI.Util;
using HealthCheck.WebUI.ViewModels;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Reflection;
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
        /// Contains services that enables extra functionality.
        /// </summary>
        protected HealthCheckServiceContainer<TAccessRole> Services { get; } = new HealthCheckServiceContainer<TAccessRole>();

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
        public HealthCheckControllerBase()
        {
            Helper = new HealthCheckControllerHelper<TAccessRole>(Services);
        }

        #region Modules
        /// <summary>
        /// Register a module that will be available.
        /// </summary>
        protected TModule UseModule<TModule>(TModule module, string name = null)
            where TModule: IHealthCheckModule
            => Helper.UseModule(module, name);

        /// <summary>
        /// Grants the given roles access to a module and assign the given accesses.
        /// <para>ConfigureModuleAccess(MyAccessRoles.Member, TestModule.ViewThing </para>
        /// <para>ConfigureModuleAccess(MyAccessRoles.Admin, TestModule.EditThing | TestModule.CreateThing)</para>
        /// </summary>
        protected void GiveRolesAccessToModule<TModuleAccessOptionsEnum>(TAccessRole roles, TModuleAccessOptionsEnum access)
            where TModuleAccessOptionsEnum : Enum
            => Helper.GiveRolesAccessToModule(roles, access);

        /// <summary>
        /// Grants the given roles access to a module without any specific access options.
        /// </summary>
        protected void GiveRolesAccessToModule<TModule>(TAccessRole roles)
            where TModule : IHealthCheckModule
            => Helper.GiveRolesAccessToModule<TModule>(roles);

        /// <summary>
        /// Grants the given roles access to a module with full access.
        /// </summary>
        protected void GiveRolesAccessToModuleWithFullAccess<TModule>(TAccessRole roles)
            where TModule : IHealthCheckModule
            => Helper.GiveRolesAccessToModuleWithFullAccess<TModule>(roles);

        /// <summary>
        /// Invokes a module method.
        /// </summary>
        [RequestLogInfo(hide: true)]
        public async Task<ActionResult> InvokeModuleMethod(string moduleId, string methodName, string jsonPayload)
        {
            var result = await Helper.InvokeModuleMethod(CurrentRequestInformation, moduleId, methodName, jsonPayload);
            if (!result.HasAccess)
            {
                return HttpNotFound();
            }
            return Content(result.Result);
        }
        #endregion

        #region Abstract/Virtual
        /// <summary>
        /// Returns the page html.
        /// </summary>
        [RequestLogInfo(hide: true)]
        public virtual ActionResult Index()
        {
            if (!Enabled) return HttpNotFound();
            else if (!Helper.HasAccessToAnyContent(CurrentRequestAccessRoles))
            {
                var redirectTarget = Helper.AccessOptions.RedirectTargetOnNoAccessUsingRequest?.Invoke(Request);
                if (!string.IsNullOrWhiteSpace(redirectTarget))
                {
                    return Redirect(redirectTarget);
                }
                else if (!string.IsNullOrWhiteSpace(Helper.AccessOptions.RedirectTargetOnNoAccess))
                {
                    return Redirect(Helper.AccessOptions.RedirectTargetOnNoAccess);
                }
                else
                {
                    return HttpNotFound();
                }
            }

            var frontEndOptions = GetFrontEndOptions();
            var pageOptions = GetPageOptions();
            var html = Helper.CreateViewHtml(CurrentRequestAccessRoles, frontEndOptions, pageOptions);
            return Content(html);
        }

        /// <summary>
        /// Get front-end options.
        /// </summary>
        protected abstract FrontEndOptionsViewModel GetFrontEndOptions();

        /// <summary>
        /// Get page options.
        /// </summary>
        protected abstract PageOptions GetPageOptions();

        /// <summary>
        /// Should return a custom enum flag object with the roles of the current user. Must match the type used in <see cref="RuntimeTestAttribute.RolesWithAccess"/>.
        /// </summary>
        protected abstract RequestInformation<TAccessRole> GetRequestInformation(HttpRequestBase request);

        /// <summary>
        /// Set any options here. Method is invoked from BeginExecute.
        /// </summary>
        protected abstract void ConfigureAccess(HttpRequestBase request, AccessOptions<TAccessRole> options);

        /// <summary>
        /// Calls GetRequestAccessRoles and SetOptions.
        /// </summary>
        protected override IAsyncResult BeginExecute(RequestContext requestContext, AsyncCallback callback, object state)
        {
            var request = requestContext?.HttpContext?.Request;
            CurrentRequestInformation = GetRequestInformation(request);
            CurrentRequestAccessRoles = CurrentRequestInformation?.AccessRole;
            Helper.BeforeConfigure(CurrentRequestInformation);
            ConfigureAccess(request, Helper.AccessOptions);
            Helper.AfterConfigure(CurrentRequestInformation);
            return base.BeginExecute(requestContext, callback, state);
        }
        #endregion

        #region Misc/Utils
        /// <summary>
        /// Returns 'OK' and 200 status code.
        /// </summary>
        [RequestLogInfo(hide: true)]
        public virtual ActionResult Ping()
        {
            if (!Enabled || !Helper.CanUsePingEndpoint(CurrentRequestAccessRoles))
                return HttpNotFound();

            return Content("OK");
        }
        #endregion

        #region AuditLog
        /// <summary>
        /// Get filtered audit events to show in the UI.
        /// </summary>
        [RequestLogInfo(hide: true)]
        [HttpPost]
        public virtual async Task<ActionResult> GetFilteredAudits(AuditEventFilterInputData input = null)
        {
            if (!Enabled || !Helper.CanShowPageTo(HealthCheckPageType.AuditLog, CurrentRequestAccessRoles))
                return HttpNotFound();

            var filteredItems = await Helper.GetAuditEventsFilterViewModel(CurrentRequestAccessRoles, input);
            return CreateJsonResult(filteredItems);
        }
        #endregion

        #region SiteEvents
        /// <summary>
        /// Get site events to show in the UI.
        /// </summary>
        [RequestLogInfo(hide: true)]
        public virtual async Task<ActionResult> GetSiteEvents()
        {
            if (!Enabled || !Helper.CanShowPageTo(HealthCheckPageType.Overview, CurrentRequestAccessRoles))
                return HttpNotFound();

            var viewModel = await Helper.GetSiteEventsViewModel(CurrentRequestAccessRoles);
            return CreateJsonResult(viewModel);
        }
        #endregion

        #region LogSearch
        /// <summary>
        /// Get log entry search results.
        /// </summary>
        [RequestLogInfo(hide: true)]
        [HttpPost]
        public virtual async Task<ActionResult> SearchLogs(LogSearchFilter filter)
        {
            if (!Enabled || !Helper.CanShowPageTo(HealthCheckPageType.LogViewer, CurrentRequestAccessRoles))
                return HttpNotFound();

            var result = await Helper.SearchLogs(CurrentRequestAccessRoles, filter);
            Helper.AuditLog_LogSearch(CurrentRequestInformation, filter, result);

            return CreateJsonResult(result);
        }

        /// <summary>
        /// Cancels the given log search.
        /// </summary>
        [RequestLogInfo(hide: true)]
        [HttpPost]
        public virtual async Task<ActionResult> CancelLogSearch(string searchId)
        {
            var cancelled = Helper.CancelLogSearch(searchId);
            if (cancelled)
            {
                Helper.AuditLog_LogSearchCancel(CurrentRequestInformation, "Cancelled log search");
            }
            return await Task.FromResult(CreateJsonResult(cancelled));
        }

        /// <summary>
        /// Cancels all log searches.
        /// </summary>
        [RequestLogInfo(hide: true)]
        [HttpPost]
        public virtual async Task<int> CancelAllLogSearches()
        {
            var count = Helper.CancelAllLogSearches();
            if (count > 0)
            {
                Helper.AuditLog_LogSearchCancel(CurrentRequestInformation, "Cancelled all log searches", count);
            }
            return await Task.FromResult(count);
        }
        #endregion

        #region Diagrams
        /// <summary>
        /// Get diagrams to show in the UI.
        /// </summary>
        [RequestLogInfo(hide: true)]
        public virtual ActionResult GetDiagrams()
        {   
            if (!Enabled || !Helper.CanShowPageTo(HealthCheckPageType.Documentation, CurrentRequestAccessRoles)) return HttpNotFound();

            var viewModel = Helper.GetDiagramsViewModel(CurrentRequestAccessRoles);
            return CreateJsonResult(viewModel);
        }
        #endregion

        #region Dataflow
        /// <summary>
        /// Get dataflow streams metadata to show in the UI.
        /// </summary>
        [RequestLogInfo(hide: true)]
        public virtual ActionResult GetDataflowStreamsMetadata()
        {
            if (!Enabled || !Helper.CanShowDataflowPageTo(CurrentRequestAccessRoles)) return HttpNotFound();

            var viewModel = Helper.GetDataflowStreamsMetadata(CurrentRequestAccessRoles);
            return CreateJsonResult(viewModel);
        }

        /// <summary>
        /// Get dataflow streams metadata to show in the UI.
        /// </summary>
        [RequestLogInfo(hide: true)]
        [HttpPost]
        public virtual async Task<ActionResult> GetDataflowStreamEntries(GetDataflowStreamEntriesFilter filter)
        {
            if (!Enabled || !Helper.CanShowDataflowPageTo(CurrentRequestAccessRoles)) return HttpNotFound();

            var viewModel = await Helper.GetDataflowEntries(filter.StreamId, filter.StreamFilter, CurrentRequestInformation);
            return CreateJsonResult(viewModel);
        }
        #endregion

        #region EventNotificationConfig
        /// <summary>
        /// Get viewmodel for the event notification configs
        /// </summary>
        [RequestLogInfo(hide: true)]
        public virtual ActionResult GetEventNotificationConfigs()
        {
            if (!Enabled || !Helper.CanShowPageTo(HealthCheckPageType.EventNotifications, CurrentRequestAccessRoles)) return HttpNotFound();

            var viewModel = Helper.GetEventNotificationConfigs(CurrentRequestAccessRoles);
            return CreateJsonResult(viewModel);
        }

        /// <summary>
        /// Delete the event notification config with the given id.
        /// </summary>
        [RequestLogInfo(hide: true)]
        [HttpPost]
        public virtual ActionResult DeleteEventNotificationConfig(Guid configId)
        {
            if (!Enabled || !Helper.CanShowPageTo(HealthCheckPageType.EventNotifications, CurrentRequestAccessRoles)) return HttpNotFound();

            var success = Helper.DeleteEventNotificationConfig(CurrentRequestInformation, configId);
            return CreateJsonResult(new { Success = success });
        }

        /// <summary>
        /// Enable/disable notification config with the given id.
        /// </summary>
        [RequestLogInfo(hide: true)]
        [HttpPost]
        public virtual ActionResult SetEventNotificationConfigEnabled(Guid configId, bool enabled)
        {
            if (!Enabled || !Helper.CanShowPageTo(HealthCheckPageType.EventNotifications, CurrentRequestAccessRoles)) return HttpNotFound();

            var success = Helper.SetEventNotificationConfigEnabled(CurrentRequestInformation, configId, enabled);
            return CreateJsonResult(new { Success = success });
        }

        /// <summary>
        /// Save an event notification config.
        /// </summary>
        [RequestLogInfo(hide: true)]
        [HttpPost]
        public virtual ActionResult SaveEventNotificationConfig(EventSinkNotificationConfig config)
        {
            if (!Enabled || !Helper.CanShowPageTo(HealthCheckPageType.EventNotifications, CurrentRequestAccessRoles)) return HttpNotFound();

            config = Helper.SaveEventNotificationConfig(CurrentRequestInformation, config);
            return CreateJsonResult(config);
        }

        /// <summary>
        /// Delete a single event definition.
        /// </summary>
        [RequestLogInfo(hide: true)]
        [HttpPost]
        public virtual ActionResult DeleteEventDefinition(string eventId)
        {
            if (!Enabled || !Helper.CanEditEventDefinitions(CurrentRequestAccessRoles)) return HttpNotFound();

            Helper.DeleteEventDefinition(CurrentRequestInformation, eventId);
            return CreateJsonResult(true);
        }

        /// <summary>
        /// Delete all event definitions.
        /// </summary>
        [RequestLogInfo(hide: true)]
        [HttpPost]
        public virtual ActionResult DeleteEventDefinitions()
        {
            if (!Enabled || !Helper.CanEditEventDefinitions(CurrentRequestAccessRoles)) return HttpNotFound();

            Helper.DeleteEventDefinitions(CurrentRequestInformation);
            return CreateJsonResult(true);
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
