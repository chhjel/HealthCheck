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
        /// <para> Uses <see cref="DefaultMergeLogic"/> with allowExtendPastCurrentTime = false by default.</para>
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
        /// <para> Uses <see cref="DefaultMergeLogic"/> with allowExtendPastCurrentTime = false by default.</para>
        /// </param>
        public SiteEventMergeOptions(bool allowEventMerge, int maxMinutesSinceLastEventEnd,
            float? lastEventDurationMultiplier = null,
            Action<SiteEvent, SiteEvent> eventMerger = null)
        {
            AllowEventMerge = allowEventMerge;
            MaxMinutesSinceLastEventEnd = maxMinutesSinceLastEventEnd;
            LastEventDurationMultiplier = lastEventDurationMultiplier;
            EventMerger = eventMerger ?? new Action<SiteEvent, SiteEvent>((old, nw) => DefaultMergeLogic(old, nw));
        }

        /// <summary>
        /// The default method that is called when merging events.
        /// Sets the old event duration to the highest of either (old.time + old.duration + new.duration) or the new events timestamp.
        /// <para>Duration will not be increased past the current time, and duration will not be decreased.</para>
        /// <para>Also updates developer details if not null.</para>
        /// </summary>
        public static void DefaultMergeLogic(SiteEvent existingEvent, SiteEvent newEvent, bool allowExtendPastCurrentTime = false)
        {
            // Minutes until new event start
            var minutesToAddUntilNewEventStart = (int)(newEvent.Timestamp - existingEvent.Timestamp).TotalMinutes;
            // Minutes until extended old event
            var minutesToAddToExtendedOldEvent = newEvent.Duration;

            // Find the highest of the two
            var minutesToAdd = Math.Max(minutesToAddUntilNewEventStart, minutesToAddToExtendedOldEvent);

            // Limit to minutes until current time.
            if (!allowExtendPastCurrentTime)
            {
                var minutesUntilCurrentTime = (int)(DateTime.Now - existingEvent.Timestamp.AddMinutes(existingEvent.Duration)).TotalMinutes;
                minutesToAdd = Math.Min(minutesUntilCurrentTime, minutesToAdd);
            }

            // Extend duration if suitable
            if (minutesToAdd > 0)
            {
                existingEvent.Duration += minutesToAdd;
            }

            // Update developer details
            if (newEvent.DeveloperDetails != null)
            {
                existingEvent.DeveloperDetails = newEvent.DeveloperDetails;
            }
        }
    }
}
