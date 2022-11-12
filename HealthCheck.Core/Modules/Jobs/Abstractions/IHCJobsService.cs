using HealthCheck.Core.Modules.Jobs.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HealthCheck.Core.Modules.Jobs.Abstractions
{
    /// <summary>
    /// 
    /// </summary>
    public interface IHCJobsService
    {
        /// <summary></summary>
        Task<List<HCJobDefinitionWithSource>> GetJobDefinitions();

        /// <summary></summary>
        Task<List<HCJobHistoryEntry>> GetLatestHistoryPerJobIdAsync();
        /// <summary></summary>
        Task<HCPagedJobHistoryEntry> GetPagedHistoryAsync(string sourceId, string jobId, int pageIndex, int pageSize);

        /// <summary></summary>
        Task<HCPagedJobLogItems> GetPagedJobLogItemsAsync(string sourceId, string jobId, int pageIndex, int pageSize);

        /// <summary></summary>
        Task<HCJobHistoryDetailEntry> GetHistoryDetailAsync(Guid id);

        /// <summary></summary>
        Task<List<HCJobStatus>> GetJobStatusesAsync();
        /// <summary></summary>
        Task<HCJobStatus> GetJobStatusAsync(string sourceId, string jobId);

        /// <summary></summary>
        Task<HCJobStartResult> StartJobAsync(string sourceId, string jobId, Dictionary<string, string> parameters);
        /// <summary></summary>
        Task<HCJobStopResult> StopJobAsync(string sourceId, string jobId);

        /// <summary></summary>
        Task<HCJobSimpleResult> DeleteHistoryItemAsync(Guid id, Func<HCJobHistoryEntry, bool> condition);
        /// <summary></summary>
        Task<HCJobSimpleResult> DeleteAllHistoryForJobAsync(string sourceId, string jobId);
        /// <summary></summary>
        Task<HCJobSimpleResult> DeleteAllHistoryAsync();
    }
}
