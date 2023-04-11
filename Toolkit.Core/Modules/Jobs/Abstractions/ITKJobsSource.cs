using QoDL.Toolkit.Core.Modules.Jobs.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace QoDL.Toolkit.Core.Modules.Jobs.Abstractions;

/// <summary>
/// 
/// </summary>
public interface ITKJobsSource
{
    /// <summary></summary>
    string Name { get; }

    /// <summary></summary>
    string Description { get; }

    /// <summary></summary>
    Task<List<TKJobDefinition>> GetJobDefinitionsAsync();

    /// <summary></summary>
    Task<TKPagedJobLogItems> GetJobLogItemsPaged(string jobId, int pageIndex, int pageSize);

    /// <summary></summary>
    Task<TKJobStatus> GetJobStatusAsync(string id);

    /// <summary></summary>
    Task<List<TKJobStatus>> GetJobStatusesAsync();

    /// <summary></summary>
    Task<TKJobStartResult> StartJobAsync(string jobId, TKJobStartCustomParameters parameters);

    /// <summary></summary>
    Task<TKJobStopResult> StopJobAsync(string jobId);
}
