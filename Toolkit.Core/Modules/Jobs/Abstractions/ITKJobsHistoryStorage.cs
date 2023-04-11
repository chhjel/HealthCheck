using QoDL.Toolkit.Core.Modules.Jobs.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace QoDL.Toolkit.Core.Modules.Jobs.Abstractions
{
    /// <summary>
    /// 
    /// </summary>
    public interface ITKJobsHistoryStorage
    {
        /// <summary></summary>
        Task<TKJobHistoryEntry> InsertHistoryAsync(TKJobHistoryEntry history);

        /// <summary></summary>
        Task<TKPagedJobHistoryEntry> GetPagedHistoryAsync(string sourceId, string jobId, int pageIndex, int pageSize);

        /// <summary></summary>
        Task<List<TKJobHistoryEntry>> GetLatestHistoryPerJobIdAsync();

        /// <summary></summary>
        Task<TKJobHistoryEntry> GetHistory(Guid id);

        /// <summary></summary>
        Task DeleteHistoryItemAsync(Guid id);

        /// <summary></summary>
        Task DeleteAllHistoryForJobAsync(string sourceId, string jobId);

        /// <summary></summary>
        Task DeleteAllHistoryAsync();

        /// <summary></summary>
        Task<IEnumerable<TKJobHistoryEntry>> LimitMaxHistoryCountForJob(string sourceId, string jobId, int maxCount);
    }
}
