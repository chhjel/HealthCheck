using HealthCheck.Core.Abstractions.Modules;
using System;
using System.Collections.Generic;

namespace HealthCheck.Module.EndpointControl.Module
{
    /// <summary>
    /// Module for controlling specified endpoints.
    /// </summary>
    public class HCEndpointControlModule : HealthCheckModuleBase<HCEndpointControlModule.AccessOption>
    {
        private HCEndpointControlModuleOptions Options { get; }

        /// <summary>
        /// Module for controlling specified endpoints.
        /// </summary>
        public HCEndpointControlModule(HCEndpointControlModuleOptions options)
        {
            Options = options;
        }

        /// <summary>
        /// Check options object for issues.
        /// </summary>
        public override IEnumerable<string> Validate()
        {
            var issues = new List<string>();
            if (Options.EndpointControlService == null) issues.Add("Options.EndpointControlService must be set.");
            return issues;
        }

        /// <summary>
        /// Get frontend options for this module.
        /// </summary>
        public override object GetFrontendOptionsObject(HealthCheckModuleContext context) => null;

        /// <summary>
        /// Get config for this module.
        /// </summary>
        public override IHealthCheckModuleConfig GetModuleConfig(HealthCheckModuleContext context) => new HCEndpointControlModuleConfig();
        
        /// <summary>
        /// Different access options for this module.
        /// </summary>
        [Flags]
        public enum AccessOption
        {
            /// <summary>Does nothing.</summary>
            None = 0,
        }

        #region Invokable 
        ///// <summary>
        ///// Get viewmodel for the event notification configs
        ///// </summary>
        //[HealthCheckModuleMethod]
        //public GetEventNotificationConfigsViewModel GetEventNotificationConfigs()
        //{
        //    var notifiers = Options.EventSink.GetNotifiers();
        //    var configs = Options.EventSink.GetConfigs();
        //    var definitions = Options.EventSink.GetKnownEventDefinitions();
        //    var placeholders = Options.EventSink.GetPlaceholders();

        //    return new GetEventNotificationConfigsViewModel()
        //    {
        //        Notifiers = notifiers.Select(x => new EventNotifierViewModel(x)),
        //        Configs = configs,
        //        KnownEventDefinitions = definitions,
        //        Placeholders = placeholders
        //    };
        //}

        ///// <summary>
        ///// Enable/disable notification config with the given id.
        ///// </summary>
        //[HealthCheckModuleMethod]
        //public object SetEventNotificationConfigEnabled(HealthCheckModuleContext context, SetEventNotificationConfigEnabledRequestModel model)
        //{
        //    var config = Options.EventSink.GetConfigs().FirstOrDefault(x => x.Id == model.ConfigId);
        //    if (config == null)
        //        return new { Success = false };

        //    config.Enabled = model.Enabled;
        //    config.LastChangedBy = context?.UserName ?? "Anonymous";
        //    config.LastChangedAt = DateTimeOffset.Now;

        //    config = Options.EventSink.SaveConfig(config);

        //    context?.AddAuditEvent($"{(model.Enabled ? "Enabled" : "Disabled")} event notification config", config.Id.ToString());
        //    return new { Success = true };
        //}
        #endregion
    }
}
