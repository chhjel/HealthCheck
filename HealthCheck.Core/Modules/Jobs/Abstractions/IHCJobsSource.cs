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
        public string Description { get; }

        /// <summary></summary>
        Task<List<HCJobDefinition>> GetJobDefinitionsAsync();

        /// <summary></summary>
        Task<HCJobStatus> GetJobStatusAsync(string id);

        /// <summary></summary>
        Task<List<HCJobStatus>> GetJobStatusesAsync();

        /// <summary>
        /// E.g: 1. save parameters, 2. start job, 3. job loads parameters
        /// </summary>
        Task<HCJobStartResult> StartJobAsync(string jobId, object parameters);

        /// <summary></summary>
        Task<HCJobStopResult> StopJobAsync(string jobId);
    }
}
