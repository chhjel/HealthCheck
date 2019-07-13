using HealthCheck.Core.Entities;
using System;

namespace HealthCheck.Core.Services.Models
{
    /// <summary>
    /// Options related to the merging of close events.
    /// </summary>
    public class SiteEventMergeOptions
    {
        /// <summary>
        /// Allow merging events with close timestamps and equal <see cref="SiteEvent.EventTypeId"/>.
        /// <para>True by default.</para>
        /// </summary>
        public bool AllowEventMerge { get; set; } = true;

        /// <summary>
        /// Merge will only be allowed if the time since the last event ended equals this or less.
        /// </summary>
        public int MaxMinutesSinceLastEventEnd { get; set; }

        /// <summary>
        /// Last event duration will be multiplied by this value for threshold calculation purposes.
        /// </summary>
        public float? LastEventDurationMultiplier { get; set; }

        /// <summary>
        /// Logic for merging two close events.
        /// <para>Parameter 1 = existing event that will be updated.</para>
        /// <para>Parameter 2 = new event data that should be merged into existing one.</para>
        /// <para>By default only duration will be extended of the original one.</para>
        /// </summary>
        public Action<SiteEvent, SiteEvent> EventMerger { get; set; }

        /// <summary>
        /// Options related to the merging of close events.
        /// </summary>
        /// <param name="allowEventMerge">Allow merging events with close timestamps and equal <see cref="SiteEvent.EventTypeId"/>.</param>
        /// <param name="maxMinutesSinceLastEventEnd">Merge will only be allowed if the time since the last event ended equals this or less.</param>
        /// <param name="lastEventDurationMultiplier">Last event duration will be multiplied by this value for threshold calculation purposes.</param>
        /// <param name="eventMerger">
        /// Logic for merging two close events.
        /// <para>Parameter 1 = existing event that will be updated.</para>
        /// <para>Parameter 2 = new event data that should be merged into existing one.</para>
        /// <para>By default only duration will be extended of the original one, but not past the current time.</para>
        /// </param>
        public SiteEventMergeOptions(bool allowEventMerge, int maxMinutesSinceLastEventEnd,
            float? lastEventDurationMultiplier = null,
            Action<SiteEvent, SiteEvent> eventMerger = null)
        {
            AllowEventMerge = allowEventMerge;
            MaxMinutesSinceLastEventEnd = maxMinutesSinceLastEventEnd;
            LastEventDurationMultiplier = lastEventDurationMultiplier;
            EventMerger = eventMerger ?? DefaultMergeLogic;
        }

        private static void DefaultMergeLogic(SiteEvent oldEvent, SiteEvent newEvent)
        {
            var maxAllowedMinutesToAdd = (int)(DateTime.Now - oldEvent.Timestamp.AddMinutes(oldEvent.Duration)).TotalMinutes;
            var minutesToAdd = Math.Min(newEvent.Duration, maxAllowedMinutesToAdd);
            if (minutesToAdd <= 0)
            {
                return;
            }

            oldEvent.Duration += newEvent.Duration;
        }
    }
}
