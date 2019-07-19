using HealthCheck.Core.Abstractions;
using HealthCheck.Core.Entities;
using HealthCheck.Core.Services.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HealthCheck.Core.Services
{
    /// <summary>
    /// Manages <see cref="SiteEvent"/>s.
    /// </summary>
    public class SiteEventService
    {
        /// <summary>
        /// Default options for merging of close events.
        /// </summary>
        public SiteEventMergeOptions DefaultMergeOptions { get; set; }

        private ISiteEventStorage Storage { get; set; }

        /// <summary>
        /// Create a new <see cref="SiteEventService"/> that manages <see cref="SiteEvent"/>s.
        /// </summary>
        /// <param name="storage">Implementation for event storage.</param>
        /// <param name="defaultMergeOptions">Default event merge options. Defaults to [allowEventMerge: true, maxMinutesSinceLastEventEnd: 15, lastEventDurationMultiplier: 1.2f]</param>
        public SiteEventService(ISiteEventStorage storage, SiteEventMergeOptions defaultMergeOptions = null)
        {
            Storage = storage;
            DefaultMergeOptions = defaultMergeOptions 
                ?? new SiteEventMergeOptions(
                    allowEventMerge: true,
                    maxMinutesSinceLastEventEnd: 15,
                    lastEventDurationMultiplier: 1.2f,
                    eventMerger: null
                );
        }

        /// <summary>
        /// Store a <see cref="SiteEvent"/> object.
        /// <para>Merges with the latest event with the same <see cref="SiteEvent.EventTypeId"/> if suitable.</para>
        /// </summary>
        public async Task StoreEvent(SiteEvent siteEvent, SiteEventMergeOptions forcedMergeOptions = null)
        {
            var mergeOptions = forcedMergeOptions ?? DefaultMergeOptions;
            var shouldUpdate = false;

            var lastEvent = await Storage.GetLastMergableEventOfType(siteEvent.EventTypeId);
            if (mergeOptions.AllowEventMerge && lastEvent != null)
            {
                var lastEventDuration = lastEvent.Duration;
                if (mergeOptions.LastEventDurationMultiplier != null)
                {
                    lastEventDuration = (int)((float)lastEventDuration * mergeOptions.LastEventDurationMultiplier);
                }

                var lastCalculatedEndTime = lastEvent.Timestamp.AddMinutes(lastEventDuration);
                var minutesBetweenEvents = siteEvent.Timestamp - lastCalculatedEndTime;
                if (minutesBetweenEvents.TotalMinutes <= mergeOptions.MaxMinutesSinceLastEventEnd)
                {
                    shouldUpdate = true;
                    DefaultMergeOptions.EventMerger(lastEvent, siteEvent);
                }
            }

            if (shouldUpdate)
            {
                await Storage.UpdateEvent(lastEvent);
            }
            else
            {
                await Storage.StoreEvent(siteEvent);
            }
        }

        /// <summary>
        /// Mark the last event with the given <paramref name="eventTypeId"/> as resolved with the given message.
        /// </summary>
        public async Task<bool> MarkEventAsResolved(string eventTypeId, string resolveMessage)
        {
            var lastEvent = await Storage.GetLastEventOfType(eventTypeId);
            if (lastEvent == null || lastEvent.Resolved)
            {
                return false;
            }

            lastEvent.Resolved = true;
            lastEvent.ResolvedAt = DateTime.Now;
            lastEvent.ResolvedMessage = resolveMessage;
            await Storage.UpdateEvent(lastEvent);
            return true;
        }

        /// <summary>
        /// Get all stored <see cref="SiteEvent"/>s objects with a <see cref="SiteEvent.Timestamp"/> within the given threshold.
        /// </summary>
        public async Task<List<SiteEvent>> GetEvents(DateTime from, DateTime to)
            => await Storage.GetEvents(from, to);
    }
}
