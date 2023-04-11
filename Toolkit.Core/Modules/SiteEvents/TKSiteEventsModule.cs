using QoDL.Toolkit.Core.Abstractions.Modules;
using QoDL.Toolkit.Core.Modules.SiteEvents.Factories;
using QoDL.Toolkit.Core.Modules.SiteEvents.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QoDL.Toolkit.Core.Modules.SiteEvents
{
    /// <summary>
    /// Module that gives an overview of stability by showing reported events.
    /// </summary>
    public class TKSiteEventsModule : ToolkitModuleBase<TKSiteEventsModule.AccessOption>
    {
        private TKSiteEventsModuleOptions Options { get; }

        private readonly SiteEventViewModelsFactory SiteEventViewModelsFactory = new();

        /// <summary>
        /// Module that gives an overview of stability by showing reported events.
        /// </summary>
        public TKSiteEventsModule(TKSiteEventsModuleOptions options)
        {
            Options = options;
        }

        /// <summary>
        /// Check options object for issues.
        /// </summary>
        public override IEnumerable<string> Validate()
        {
            var issues = new List<string>();
            if (Options.SiteEventService == null) issues.Add("Options.SiteEventService must be set.");
            return issues;
        }

        /// <summary>
        /// Get frontend options for this module.
        /// </summary>
        public override object GetFrontendOptionsObject(ToolkitModuleContext context) => new TKSiteEventsModuleFrontendOptionsModel
        {
            FrontendAutoRefreshSecondsInterval = Options.FrontendAutoRefreshSecondsInterval,
            CustomHtml = Options.CustomHtml,
            ShowFilter = Options.ShowFilter,
            Sections = Options.Sections
        };

        /// <summary>
        /// Get config for this module.
        /// </summary>
        public override IToolkitModuleConfig GetModuleConfig(ToolkitModuleContext context) => new TKSiteEventsModuleConfig();

        /// <summary>
        /// Different access options for this module.
        /// </summary>
        [Flags]
        public enum AccessOption
        {
            /// <summary>Does nothing.</summary>
            None = 0,

            /// <summary>Include stacktrace in some views.</summary>
            DeveloperDetails = 1,

            /// <summary>Delete stored events.</summary>
            DeleteEvents = 2,
        }

        #region Invokable methods
        /// <summary>
        /// Get all events within the given period.
        /// </summary>
        [ToolkitModuleMethod]
        public async Task<List<SiteEventViewModel>> GetSiteEvents(ToolkitModuleContext context, GetSiteEventsRequestModel model)
        {
            var includeDeveloperDetails = context.HasAccess(AccessOption.DeveloperDetails);
            model.From ??= DateTimeOffset.Now.AddDays(-30);
            model.To ??= DateTimeOffset.Now;

            var events = await Options.SiteEventService.GetEvents(model.From.Value, model.To.Value);
            var viewModel = events
                .Where(x => x.MinimumDurationRequiredToDisplay == null || x.Duration >= x.MinimumDurationRequiredToDisplay)
                .Select(x => SiteEventViewModelsFactory.CreateViewModel(x, includeDeveloperDetails))
                .ToList();
            return viewModel;
        }

        /// <summary>
        /// Clears all site events.
        /// </summary>
        [ToolkitModuleMethod(AccessOption.DeleteEvents)]
        public async Task<bool> ClearSiteEvents(ToolkitModuleContext context)
        {
            await Options.SiteEventService.DeleteAllEvents();
            context.AddAuditEvent(action: "Cleared all site events");
            return true;
        }

        /// <summary>
        /// Clears a single site event.
        /// </summary>
        [ToolkitModuleMethod(AccessOption.DeleteEvents)]
        public async Task<bool> DeleteSiteEvent(ToolkitModuleContext context, DeleteSiteEventRequestModel model)
        {
            await Options.SiteEventService.DeleteEvent(model.Id);
            context.AddAuditEvent(action: "Delete site event", subject: model.Id.ToString())
                .AddDetail("Event id", model.Id.ToString());
            return true;
        }
        #endregion
    }
}
