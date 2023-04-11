using System;

namespace QoDL.Toolkit.Core.Util
{
    /// <summary>
    /// Options for automatic storage cleanup.
    /// </summary>
    public class TKSimpleBlobStoreRetentionOptions
    {
        /// <summary>
        /// Max time since last file write of blobs before they can become deleted.
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
        /// <param name="maxAge">Max age of entries before they can become deleted.</param>
        /// <param name="minimumCleanupInterval">Cleanup logic will be executed after insertion if this duration has passed since the last cleanup.</param>
        /// <param name="delayFirstCleanup">Delay first cleanup by MinimumCleanupInterval.</param>
        public TKSimpleBlobStoreRetentionOptions(
            TimeSpan maxAge, TimeSpan minimumCleanupInterval,
            bool delayFirstCleanup = false)
        {
            MaxItemAge = maxAge;
            MinimumCleanupInterval = minimumCleanupInterval;
            DelayFirstCleanupByMinimumCleanupInterval = delayFirstCleanup;
        }
    }
}
