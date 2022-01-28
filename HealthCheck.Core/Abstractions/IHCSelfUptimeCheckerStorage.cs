﻿using System;
using System.Threading.Tasks;

namespace HealthCheck.Core.Abstractions
{
    /// <summary>
    /// Storage for last uptime check.
    /// </summary>
    public interface IHCSelfUptimeCheckerStorage
    {
        /// <summary>
        /// Get the last checked time or null if none.
        /// </summary>
        Task<DateTimeOffset?> GetLastCheckedAtAsync();

        /// <summary>
        /// Store the given last checked time.
        /// </summary>
        Task StoreLastCheckedAtAsync(DateTimeOffset time);
    }
}
