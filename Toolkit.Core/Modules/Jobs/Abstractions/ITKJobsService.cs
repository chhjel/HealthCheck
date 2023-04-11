using QoDL.Toolkit.Core.Modules.Jobs.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace QoDL.Toolkit.Core.Modules.Jobs.Abstractions;

/// <summary>
/// 
/// </summary>
public interface ITKJobsService
{
    /// <summary></summary>
    Task<List<TKJobDefinitionWithSource>> GetJobDefinitions();

    /// <summary></summary>
    Task<List<TKJobHistoryEntry>> GetLatestHistoryPerJobIdAsync();
    /// <summary></summary>
    Task<TKPagedJobHistoryEntry> GetPagedHistoryAsync(string sourceId, string jobId, int pageIndex, int pageSize);

    /// <summary></summary>
    Task<TKPagedJobLogItems> GetPagedJobLogItemsAsync(string sourceId, string jobId, int pageIndex, int pageSize);

    /// <summary></summary>
    Task<TKJobHistoryDetailEntry> GetHistoryDetailAsync(Guid id);

    /// <summary></summary>
    Task<List<TKJobStatus>> GetJobStatusesAsync();
    /// <summary></summary>
    Task<TKJobStatus> GetJobStatusAsync(string sourceId, string jobId);

    /// <summary></summary>
    Task<TKJobStartResult> StartJobAsync(string sourceId, string jobId, Dictionary<string, string> parameters);
    /// <summary></summary>
    Task<TKJobStopResult> StopJobAsync(string sourceId, string jobId);

    /// <summary></summary>
    Task<TKJobSimpleResult> DeleteHistoryItemAsync(Guid id, Func<TKJobHistoryEntry, bool> condition);
    /// <summary></summary>
    Task<TKJobSimpleResult> DeleteAllHistoryForJobAsync(string sourceId, string jobId);
    /// <summary></summary>
    Task<TKJobSimpleResult> DeleteAllHistoryAsync();
}
