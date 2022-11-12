using HealthCheck.Core.Modules.Jobs.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HealthCheck.Core.Modules.Jobs.Abstractions
{
    /// <summary>
    /// 
    /// </summary>
    public interface IHCJobsSource
    {
        /// <summary></summary>
        string Name { get; }

        /// <summary></summary>
        string Description { get; }

        /// <summary></summary>
        Task<List<HCJobDefinition>> GetJobDefinitionsAsync();

        /// <summary></summary>
        Task<HCPagedJobLogItems> GetJobLogItemsPaged(string jobId, int pageIndex, int pageSize);

        /// <summary></summary>
        Task<HCJobStatus> GetJobStatusAsync(string id);

        /// <summary></summary>
        Task<List<HCJobStatus>> GetJobStatusesAsync();

        /// <summary></summary>
        Task<HCJobStartResult> StartJobAsync(string jobId, HCJobStartCustomParameters parameters);

        /// <summary></summary>
        Task<HCJobStopResult> StopJobAsync(string jobId);
    }
}
