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
        Task<HCPagedJobHistoryEntry> GetPagedHistoryAsync(string jobId, int pageIndex, int pageSize);
        /// <summary></summary>
        Task<HCJobHistoryDetailEntry> GetHistoryDetailAsync(Guid id);
        /// <summary></summary>
        Task<List<HCJobDefinitionWithSource>> GetJobDefinitions();
        /// <summary></summary>
        Task<List<HCJobHistoryEntry>> GetLatestHistoryPerJobIdAsync();
        /// <summary></summary>
        Task<List<HCJobStatus>> GetJobStatusesAsync();
        /// <summary></summary>
        Task<HCJobStatus> GetJobStatusAsync(string sourceId, string jobId);
        /// <summary></summary>
        Task<HCJobStartResult> StartJobAsync(string sourceId, string jobId, object parameters);
        /// <summary></summary>
        Task<HCJobStopResult> StopJobAsync(string sourceId, string jobId);
    }
}
