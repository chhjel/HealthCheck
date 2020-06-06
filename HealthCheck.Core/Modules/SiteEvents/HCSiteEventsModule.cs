using HealthCheck.Core.Abstractions.Modules;
using HealthCheck.Core.Modules.SiteEvents.Factories;
using HealthCheck.Core.Modules.SiteEvents.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HealthCheck.Core.Modules.SiteEvents
{
    /// <summary>
    /// Module that gives an overview of stability by showing reported events.
    /// </summary>
    public class HCSiteEventsModule : HealthCheckModuleBase<HCSiteEventsModule.AccessOption>
    {
        private HCSiteEventsModuleOptions Options { get; }

        private readonly SiteEventViewModelsFactory SiteEventViewModelsFactory = new SiteEventViewModelsFactory();

        /// <summary>
        /// Module that gives an overview of stability by showing reported events.
        /// </summary>
        public HCSiteEventsModule(HCSiteEventsModuleOptions options)
        {
            Options = options;
        }

        /// <summary>
        /// Check options object for issues.
        /// </summary>
        public override List<string> Validate()
        {
            var issues = new List<string>();
            if (Options.SiteEventService == null) issues.Add("Options.SiteEventService must be set.");
            return issues;
        }

        /// <summary>
        /// Get frontend options for this module.
        /// </summary>
        public override object GetFrontendOptionsObject(HealthCheckModuleContext context) => new
        {
            CurrentEventBufferMinutes = Options.CurrentEventBufferMinutes
        };

        /// <summary>
        /// Get config for this module.
        /// </summary>
        public override IHealthCheckModuleConfig GetModuleConfig(HealthCheckModuleContext context) => new HCSiteEventsModuleConfig();

        /// <summary>
        /// Different access options for this module.
        /// </summary>
        [Flags]
        public enum AccessOption
        {
            /// <summary>Does nothing.</summary>
            Nothing = 0,

            /// <summary>Include stacktrace in some views.</summary>
            DeveloperDetails = 1,
        }

        #region Invokable methods
        /// <summary>
        /// Get all available tests for the current request.
        /// </summary>
        [HealthCheckModuleMethod]
        public async Task<List<SiteEventViewModel>> GetSiteEvents(HealthCheckModuleContext context, GetSiteEventsRequestModel model)
        {
            var includeDeveloperDetails = context.HasAccess(AccessOption.DeveloperDetails);
            model.From ??= DateTimeOffset.Now.AddDays(-30);
            model.To ??= DateTimeOffset.Now;

            var viewModel = (await Options.SiteEventService.GetEvents(model.From.Value, model.To.Value))
                .Select(x => SiteEventViewModelsFactory.CreateViewModel(x, includeDeveloperDetails))
                .ToList();
            return viewModel;
        }
        #endregion
    }
}
