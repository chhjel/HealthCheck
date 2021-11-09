using HealthCheck.Core.Modules.SiteEvents.Abstractions;
using HealthCheck.Core.Modules.SiteEvents.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HealthCheck.Core.Modules.SiteEvents.Services
{
    /// <summary>
    /// Default implementation for managing <see cref="SiteEvent"/>s.
    /// </summary>
    public class SiteEventService : ISiteEventService
    {
        /// <summary>
        /// Default options for merging of close events.
        /// Defaults to [allowEventMerge: true, maxMinutesSinceLastEventEnd: 15, lastEventDurationMultiplier: 1.2f]
        /// </summary>
        public SiteEventMergeOptions DefaultMergeOptions { get; set; }
            = new SiteEventMergeOptions(
                allowEventMerge: true,
                maxMinutesSinceLastEventEnd: 15,
                lastEventDurationMultiplier: 1.2f,
                eventMerger: null
            );

        private ISiteEventStorage Storage { get; set; }

        /// <summary>
        /// Create a new <see cref="SiteEventService"/> that manages <see cref="SiteEvent"/>s.
        /// </summary>
        /// <param name="storage">Implementation for event storage.</param>
        public SiteEventService(ISiteEventStorage storage)
        {
            Storage = storage;
        }

        /// <summary>
        /// Sets default event merge options.
        /// Defaults to [allowEventMerge: true, maxMinutesSinceLastEventEnd: 15, lastEventDurationMultiplier: 1.2f]
        /// </summary>
        public SiteEventService SetDefaultMergeOptions(SiteEventMergeOptions options)
        {
            DefaultMergeOptions = options;
            return this;
        }

        /// <summary>
        /// Store a <see cref="SiteEvent"/> object.
        /// <para>Merges with the latest event with the same <see cref="SiteEvent.EventTypeId"/> if suitable.</para>
        /// </summary>
        public virtual async Task StoreEvent(SiteEvent siteEvent, SiteEventMergeOptions forcedMergeOptions = null)
        {
            var mergeOptions = forcedMergeOptions ?? DefaultMergeOptions;
            var shouldUpdate = false;

            try
            {
                _storeSemaphore.WaitOne();

                SiteEvent lastEvent = null;
                if (!string.IsNullOrWhiteSpace(siteEvent.EventTypeId))
                {
                    lastEvent = await Storage.GetLastMergableEventOfType(siteEvent.EventTypeId).ConfigureAwait(false);
                }

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
                    await Storage.UpdateEvent(lastEvent).ConfigureAwait(false);
                }
                else
                {
                    await Storage.StoreEvent(siteEvent).ConfigureAwait(false);
                }
            }
            finally
            {
                _storeSemaphore.Release();
            }
        }
        private static readonly Semaphore _storeSemaphore = new(1, 1);

        /// <summary>
        /// Mark the last event with the given <paramref name="eventTypeId"/> as resolved with the given message.
        /// </summary>
        public virtual async Task<bool> MarkEventAsResolved(string eventTypeId, string resolveMessage, Action<SiteEvent> config = null)
        {
            var lastEvent = await Storage.GetLastEventOfType(eventTypeId);
            if (lastEvent == null || lastEvent.Resolved)
            {
                return false;
            }

            lastEvent.Resolved = true;
            lastEvent.ResolvedAt = DateTimeOffset.Now;
            lastEvent.ResolvedMessage = resolveMessage;
            config?.Invoke(lastEvent);
            await Storage.UpdateEvent(lastEvent);
            return true;
        }

        /// <summary>
        /// Get all stored <see cref="SiteEvent"/>s objects with a <see cref="SiteEvent.Timestamp"/> within the given threshold.
        /// </summary>
        public virtual async Task<List<SiteEvent>> GetEvents(DateTimeOffset from, DateTimeOffset to)
            => await Storage.GetEvents(from, to);

        /// <inheritdoc />
        public virtual async Task<List<SiteEvent>> GetUnresolvedEvents(DateTimeOffset? from = null, DateTimeOffset? to = null)
        {
            var all = await Storage.GetEvents(from ?? DateTimeOffset.MinValue, to ?? DateTimeOffset.MaxValue);
            return all.Where(x => !x.Resolved).ToList();
        }


        /// <inheritdoc />
        public virtual async Task DeleteAllEvents()
            => await Storage.DeleteAllEvents().ConfigureAwait(false);

        /// <inheritdoc />
        public virtual async Task DeleteEvent(Guid id)
            => await Storage.DeleteEvent(id).ConfigureAwait(false);
    }
}
