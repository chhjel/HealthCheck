using System;

namespace HealthCheck.Core.Util
{
    /// <summary>
    /// Options for automatic storage cleanup.
    /// </summary>
    public class StorageRetentionOptions<TItem>
    {
        /// <summary>
        /// Timestamp selector for stored items. Used to check age of items for cleanup.
        /// </summary>
        public Func<TItem, DateTime> ItemTimestampSelector { get; set; }

        /// <summary>
        /// Max age of entries before they can become deleted.
        /// </summary>
        public TimeSpan MaxItemAge { get; set; }

        /// <summary>
        /// Cleanup logic will be executed after insertion if this duration has passed since the last cleanup.
        /// </summary>
        public TimeSpan MinimumCleanupInterval { get; set; }

        /// <summary>
        /// Delay first cleanup by MinimumCleanupInterval.
        /// </summary>
        public bool DelayFirstCleanupByMinimumCleanupInterval { get; set; }

        /// <summary>
        /// Options for automatic storage cleanup.
        /// </summary>
        /// <param name="timestampSelector">Timestamp selector for stored items. Used to check age of items for cleanup.</param>
        /// <param name="maxAge">Max age of entries before they can become deleted.</param>
        /// <param name="minimumCleanupInterval">Cleanup logic will be executed after insertion if this duration has passed since the last cleanup.</param>
        /// <param name="delayFirstCleanup">Delay first cleanup by MinimumCleanupInterval.</param>
        public StorageRetentionOptions(Func<TItem, DateTime> timestampSelector,
            TimeSpan maxAge, TimeSpan minimumCleanupInterval,
            bool delayFirstCleanup = false)
        {
            ItemTimestampSelector = timestampSelector;
            MaxItemAge = maxAge;
            MinimumCleanupInterval = minimumCleanupInterval;
            DelayFirstCleanupByMinimumCleanupInterval = delayFirstCleanup;
        }
    }
}
