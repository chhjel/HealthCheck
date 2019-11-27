﻿namespace HealthCheck.ActionLog.Enums
{
    /// <summary>
    /// Decides what to do when the limit is reached.
    /// </summary>
    public enum TestLogCallStoragePolicy
    {
        /// <summary>
        /// Ignore any further potential entries.
        /// </summary>
        IgnoreAboveLimit = 0,

        /// <summary>
        /// Delete oldest entry, making space for the new one.
        /// </summary>
        RemoveOldest
    }
}
