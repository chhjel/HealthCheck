using QoDL.Toolkit.Core.Config;
using QoDL.Toolkit.Core.Modules.SiteEvents.Abstractions;
using QoDL.Toolkit.Core.Modules.SiteEvents.Enums;
using QoDL.Toolkit.Core.Modules.SiteEvents.Models;
using QoDL.Toolkit.Core.Util;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace QoDL.Toolkit.Core.Modules.SiteEvents.Utils
{
    /// <summary>
    /// Utilities related to the site event toolkit module.
    /// </summary>
    public static class TKSiteEventUtils
    {
        /// <summary>
        /// Attempts to register a new site event, ignoring any error that might be thrown.
        /// </summary>
        /// <param name="severity">Severity of the event.</param>
        /// <param name="eventTypeId">Custom id of this type of event.</param>
        /// <param name="title">Title of the event.</param>
        /// <param name="description">Description of the event.</param>
        /// <param name="duration">Duration of event in minutes.</param>
        /// <param name="developerDetails">Extra details for developers.</param>
        /// <param name="config">Use to add any related links etc.</param>
        public static void TryRegisterNewEvent(SiteEventSeverity severity, string eventTypeId, string title, string description,
            int duration = 1, string developerDetails = null, Action<SiteEvent> config = null)
            => TryRegisterNewEvent(new SiteEvent(severity, eventTypeId, title, description, duration, developerDetails), config);

        /// <summary>
        /// Attempts to register a new site event, ignoring any error that might be thrown.
        /// </summary>
        public static void TryRegisterNewEvent(SiteEvent siteEvent, Action<SiteEvent> config = null)
        {
            try
            {
                var service = TKGlobalConfig.GetService<ISiteEventService>();
                config?.Invoke(siteEvent);
                Task.Run(async () => await service?.StoreEvent(siteEvent));
            }
            catch (Exception ex)
            {
                TKGlobalConfig.OnExceptionEvent?.Invoke(typeof(TKSiteEventUtils), nameof(TryRegisterNewEvent), ex);
            }
        }

        /// <summary>
        /// Attempts to mark the latest event with the given event type id as resolved, ignoring any error that might be thrown.
        /// </summary>
        public static void TryMarkLatestEventAsResolved(string eventTypeId, string resolvedMessage, Action<SiteEvent> config = null)
        {
            try
            {
                var service = TKGlobalConfig.GetService<ISiteEventService>();
                Task.Run(async () => await service?.MarkLatestEventAsResolved(eventTypeId, resolvedMessage, config));
            }
            catch (Exception ex)
            {
                TKGlobalConfig.OnExceptionEvent?.Invoke(typeof(TKSiteEventUtils), nameof(TryMarkLatestEventAsResolved), ex);
            }
        }

        /// <summary>
        /// Attempts to mark all events with the given event type id as resolved, ignoring any error that might be thrown.
        /// </summary>
        public static void TryMarkAllEventsAsResolved(string eventTypeId, string resolvedMessage, Action<SiteEvent> config = null)
        {
            try
            {
                var service = TKGlobalConfig.GetService<ISiteEventService>();
                Task.Run(async () => await service?.MarkAllEventsAsResolved(eventTypeId, resolvedMessage, config));
            }
            catch (Exception ex)
            {
                TKGlobalConfig.OnExceptionEvent?.Invoke(typeof(TKSiteEventUtils), nameof(TryMarkAllEventsAsResolved), ex);
            }
        }

        /// <summary>
        /// Attempts to mark the event with the given id as resolved, ignoring any error that might be thrown.
        /// </summary>
        public static void TryMarkEventAsResolved(Guid id, string resolvedMessage, Action<SiteEvent> config = null)
        {
            try
            {
                var service = TKGlobalConfig.GetService<ISiteEventService>();
                Task.Run(async () => await service?.MarkEventAsResolved(id, resolvedMessage, config));
            }
            catch (Exception ex)
            {
                TKGlobalConfig.OnExceptionEvent?.Invoke(typeof(TKSiteEventUtils), nameof(TryMarkEventAsResolved), ex);
            }
        }

        /// <summary>
        /// Attempts to get all stored unresolved <see cref="SiteEvent"/>s objects, ignoring any error that might be thrown.
        /// </summary>
        public static List<SiteEvent> TryGetAllUnresolvedEvents()
        {
            try
            {
                var service = TKGlobalConfig.GetService<ISiteEventService>();
                if (service == null)
                {
                    return new List<SiteEvent>();
                }
                var events = TKAsyncUtils.RunSync(() => service.GetUnresolvedEvents());
                return events;
            }
            catch (Exception ex)
            {
                TKGlobalConfig.OnExceptionEvent?.Invoke(typeof(TKSiteEventUtils), nameof(TryGetAllUnresolvedEvents), ex);
                return new List<SiteEvent>();
            }
        }
    }
}
