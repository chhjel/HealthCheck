﻿using HealthCheck.Core.Modules.Jobs.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HealthCheck.Core.Modules.Jobs.Abstractions
{
    /// <summary>
    /// 
    /// </summary>
    public interface IHCJobsHistoryStorage
    {
        /// <summary></summary>
        Task<HCJobHistoryEntry> InsertHistoryAsync(HCJobHistoryEntry history);

        /// <summary></summary>
        Task<HCPagedJobHistoryEntry> GetPagedHistoryAsync(string jobId, int pageIndex, int pageSize);

        /// <summary></summary>
        Task<List<HCJobHistoryEntry>> GetLatestHistoryPerJobIdAsync();

        /// <summary></summary>
        Task DeleteHistoryItemAsync(Guid id);

        /// <summary></summary>
        Task DeleteAllHistoryForJobAsync(string jobId);

        /// <summary></summary>
        Task DeleteAllHistoryAsync();
    }
}
