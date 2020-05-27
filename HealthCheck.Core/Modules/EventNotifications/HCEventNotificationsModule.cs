using HealthCheck.Core.Abstractions.Modules;
using HealthCheck.Core.Modules.EventNotifications.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HealthCheck.Core.Modules.EventNotifications
{
    /// <summary>
    /// Module for viewing audit logs.
    /// </summary>
    public class HCEventNotificationsModule : HealthCheckModuleBase<HCEventNotificationsModule.AccessOption>
    {
        private HCEventNotificationsModuleOptions Options { get; }

        /// <summary>
        /// Module for viewing audit logs.
        /// </summary>
        public HCEventNotificationsModule(HCEventNotificationsModuleOptions options)
        {
            Options = options;
        }

        /// <summary>
        /// Get frontend options for this module.
        /// </summary>
        public override object GetFrontendOptionsObject(AccessOption access) => null;

        /// <summary>
        /// Get config for this module.
        /// </summary>
        public override IHealthCheckModuleConfig GetModuleConfig(AccessOption access) => new HCEventNotificationsModuleConfig();
        
        /// <summary>
        /// Different access options for this module.
        /// </summary>
        [Flags]
        public enum AccessOption
        {
            /// <summary>Does nothing.</summary>
            Nothing = 0,

            /// <summary>Allows editing event definitions.</summary>
            EditEventDefinitions = 1
        }

        #region Invokable 
        /// <summary>
        /// Get viewmodel for the event notification configs
        /// </summary>
        [HealthCheckModuleMethod]
        public GetEventNotificationConfigsViewModel GetEventNotificationConfigs()
        {
            var notifiers = Options.EventSink.GetNotifiers();
            var configs = Options.EventSink.GetConfigs();
            var definitions = Options.EventSink.GetKnownEventDefinitions();
            var placeholders = Options.EventSink.GetPlaceholders();

            return new GetEventNotificationConfigsViewModel()
            {
                Notifiers = notifiers.Select(x => new EventNotifierViewModel(x)),
                Configs = configs,
                KnownEventDefinitions = definitions,
                Placeholders = placeholders
            };
        }

        /// <summary>
        /// Delete the event notification config with the given id.
        /// </summary>
        [HealthCheckModuleMethod]
        public object DeleteEventNotificationConfig(HealthCheckModuleContext context, Guid configId)
        {
            var config = Options.EventSink?.GetConfigs()?.FirstOrDefault(x => x.Id == configId);
            if (config != null)
            {
                context.AddAuditEvent(action: "Deleted event notification config");
            }

            Options.EventSink.DeleteConfig(configId);
            return new { Success = true };
        }

        /// <summary>
        /// Enable/disable notification config with the given id.
        /// </summary>
        [HealthCheckModuleMethod]
        public object SetEventNotificationConfigEnabled(HealthCheckModuleContext context, SetEventNotificationConfigEnabledRequestModel model)
        {
            var config = Options.EventSink.GetConfigs().FirstOrDefault(x => x.Id == model.ConfigId);
            if (config == null)
                return new { Success = false };

            config.Enabled = model.Enabled;
            config.LastChangedBy = context?.UserName ?? "Anonymous";
            config.LastChangedAt = DateTime.Now;

            config = Options.EventSink.SaveConfig(config);

            context.AddAuditEvent($"{(model.Enabled ? "Enabled" : "Disabled")} event notification config", config.Id.ToString());
            return new { Success = true };
        }

        /// <summary>
        /// Save an event notification config.
        /// </summary>
        [HealthCheckModuleMethod]
        public EventSinkNotificationConfig SaveEventNotificationConfig(HealthCheckModuleContext context, EventSinkNotificationConfig config)
        {
            config.LastChangedBy = context?.UserName ?? "Anonymous";
            config.LastChangedAt = DateTime.Now;

            config.LatestResults ??= new List<string>();
            config.PayloadFilters ??= new List<EventSinkNotificationConfigFilter>();
            config.EventIdFilter ??= new EventSinkNotificationConfigFilter();
            config.NotifierConfigs ??= new List<NotifierConfig>();

            config = Options.EventSink.SaveConfig(config);

            if (config != null)
            {
                context.AddAuditEvent(action: "Saved event notification config");
            }
            return config;
        }

        /// <summary>
        /// Delete a single event definition.
        /// </summary>
        [HealthCheckModuleMethod(AccessOption.EditEventDefinitions)]
        public bool DeleteEventDefinition(HealthCheckModuleContext context, string eventId)
        {
            Options.EventSink?.DeleteDefinition(eventId);
            context.AddAuditEvent(action: "Delete event definition", eventId);
            return true;
        }

        /// <summary>
        /// Delete all event definitions.
        /// </summary>
        [HealthCheckModuleMethod(AccessOption.EditEventDefinitions)]
        public bool DeleteEventDefinitions(HealthCheckModuleContext context)
        {
            Options.EventSink?.DeleteDefinitions();
            context.AddAuditEvent(action: "Delete all event definitions");
            return true;
        }
        #endregion
    }
}
