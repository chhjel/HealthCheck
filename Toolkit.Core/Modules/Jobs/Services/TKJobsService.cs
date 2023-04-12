using QoDL.Toolkit.Core.Modules.Jobs.Abstractions;
using QoDL.Toolkit.Core.Modules.Jobs.Models;
using QoDL.Toolkit.Core.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QoDL.Toolkit.Core.Modules.Jobs.Services;

/// <summary>
/// 
/// </summary>
public class TKJobsService : ITKJobsService
{
    private readonly IEnumerable<ITKJobsSource> _jobSources;
    private readonly ITKJobsHistoryStorage _historyStorage;
    private readonly ITKJobsHistoryDetailsStorage _historyDetailsStorage;

    /// <summary></summary>
    public TKJobsService(IEnumerable<ITKJobsSource> jobSources, ITKJobsHistoryStorage historyStorage, ITKJobsHistoryDetailsStorage historyDetailsStorage)
    {
        _jobSources = jobSources;
        _historyStorage = historyStorage;
        _historyDetailsStorage = historyDetailsStorage;
    }

    /// <inheritdoc />
    public async Task<List<TKJobDefinitionWithSource>> GetJobDefinitions()
    {
        var defs = new List<TKJobDefinitionWithSource>();
        foreach (var source in _jobSources)
        {
            var items = (await source.GetJobDefinitionsAsync())
                .Where(x => x != null)
                .Select(x => new TKJobDefinitionWithSource
                {
                    SourceId = CreateSourceId(source),
                    Definition = x
                })
                .ToList();
            defs.AddRange(items);
        }
        return defs;
    }

    /// <inheritdoc />
    public async Task<List<TKJobHistoryEntry>> GetLatestHistoryPerJobIdAsync()
        => await _historyStorage.GetLatestHistoryPerJobIdAsync();

    /// <inheritdoc />
    public async Task<TKPagedJobHistoryEntry> GetPagedHistoryAsync(string sourceId, string jobId, int pageIndex, int pageSize)
        => await _historyStorage.GetPagedHistoryAsync(sourceId, jobId, pageIndex, pageSize);

    /// <inheritdoc />
    public async Task<TKPagedJobLogItems> GetPagedJobLogItemsAsync(string sourceId, string jobId, int pageIndex, int pageSize)
    {
        var source = _jobSources.FirstOrDefault(x => CreateSourceId(x) == sourceId);
        TKPagedJobLogItems sourceResult = new();
        if (source != null)
        {
            sourceResult = await source.GetJobLogItemsPaged(jobId, pageIndex, pageSize);
        }
        return sourceResult;
    }

    /// <inheritdoc />
    public async Task<TKJobHistoryDetailEntry> GetHistoryDetailAsync(Guid id)
        => await _historyDetailsStorage.GetDetailAsync(id);

    /// <inheritdoc />
    public async Task<TKJobStatus> GetJobStatusAsync(string sourceId, string jobId)
    {
        var source = _jobSources.FirstOrDefault(x => CreateSourceId(x) == sourceId);
        if (source == null) return null;
        var statuse = await source.GetJobStatusAsync(jobId);
        statuse.SourceId = sourceId;
        return statuse;
    }

    /// <inheritdoc />
    public async Task<List<TKJobStatus>> GetJobStatusesAsync()
    {
        var statuses = new List<TKJobStatus>();
        foreach (var source in _jobSources)
        {
            var items = (await source.GetJobStatusesAsync());
            items.ForEach(x => x.SourceId = CreateSourceId(source));
            statuses.AddRange(items);
        }
        return statuses;
    }

    /// <inheritdoc />
    public async Task<TKJobStopResult> StopJobAsync(string sourceId, string jobId)
    {
        var source = _jobSources.FirstOrDefault(x => CreateSourceId(x) == sourceId);
        if (source == null) return new TKJobStopResult { Message = "Job not found." };
        return await source.StopJobAsync(jobId);
    }

    /// <inheritdoc />
    public async Task<TKJobStartResult> StartJobAsync(string sourceId, string jobId, Dictionary<string, string> parameters)
    {
        var source = _jobSources.FirstOrDefault(x => CreateSourceId(x) == sourceId);
        if (source == null) return new TKJobStartResult { Message = "Job not found." };

        var jobs = await source.GetJobDefinitionsAsync();
        var job = jobs.FirstOrDefault(x => x.Id == jobId);
        object parametersObject = job?.CustomParametersType == null ? null : TKValueConversionUtils.ConvertInputModel(job.CustomParametersType, parameters);
        var parametersModel = new TKJobStartCustomParameters
        {
            CustomParametersRaw = parameters,
            CustomParametersInstance = parametersObject,
            CustomParametersType = job?.CustomParametersType
        };

        return await source.StartJobAsync(jobId, parametersModel);
    }

    /// <inheritdoc />
    public async Task<TKJobSimpleResult> DeleteHistoryItemAsync(Guid id, Func<TKJobHistoryEntry, bool> condition)
    {
        var item = await _historyStorage.GetHistory(id);
        if (item != null)
        {
            return TKJobSimpleResult.CreateSuccess("History is already deleted or never existed.");
        }
        if (condition?.Invoke(item) != true)
        {
            return TKJobSimpleResult.CreateError("You do not have access to delete this item.");
        }

        await _historyStorage.DeleteHistoryItemAsync(id);
        if (item.DetailId != null)
        {
            await _historyDetailsStorage.DeleteDetailAsync(item.DetailId.Value);
        }
        return TKJobSimpleResult.CreateSuccess("History item deleted.");
    }

    /// <inheritdoc />
    public async Task<TKJobSimpleResult> DeleteAllHistoryForJobAsync(string sourceId, string jobId)
    {
        await _historyStorage.DeleteAllHistoryForJobAsync(sourceId, jobId);
        await _historyDetailsStorage.DeleteAllDetailsForJobAsync(sourceId, jobId);
        return TKJobSimpleResult.CreateSuccess("All history for job deleted.");
    }

    /// <inheritdoc />
    public async Task<TKJobSimpleResult> DeleteAllHistoryAsync()
    {
        await _historyStorage.DeleteAllHistoryAsync();
        await _historyDetailsStorage.DeleteAllDetailsAsync();
        return TKJobSimpleResult.CreateSuccess("All history deleted.");
    }

    private string CreateSourceId(ITKJobsSource source) => source.GetType().FullName;
}
