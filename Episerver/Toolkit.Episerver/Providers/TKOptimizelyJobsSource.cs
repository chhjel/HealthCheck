using EPiServer.Data;
using EPiServer.Data.Dynamic;
using EPiServer.DataAbstraction;
using EPiServer.PlugIn;
using EPiServer.Scheduler;
using QoDL.Toolkit.Core.Attributes;
using QoDL.Toolkit.Core.Modules.Jobs;
using QoDL.Toolkit.Core.Modules.Jobs.Abstractions;
using QoDL.Toolkit.Core.Modules.Jobs.Attributes;
using QoDL.Toolkit.Core.Modules.Jobs.Models;
using QoDL.Toolkit.Core.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace QoDL.Toolkit.Episerver.Providers;

/// <summary>
/// Provides existing jobs from Optimizely.
/// <para>To use custom parameters decorate jobs with <see cref="TKJobAttribute"/> and specify the parameter type,
/// then get the parameters at the start of the job using <see cref="TKOptimizelyJobUtil.GetJobParameters{TJob, TParameters}"/>.</para>
/// </summary>
public class TKOptimizelyJobsSource : ITKJobsSource
{
    /// <summary></summary>
    public string Name => "Optimizely Jobs";

    /// <summary></summary>
    public string Description => "Jobs from Optimizely.";

    /// <summary>
    /// Optional filter of what jobs to include.
    /// </summary>
    public Func<PlugInDescriptor, bool> JobFilter { get; set; }

    private readonly IScheduledJobRepository _jobRepo;
    private readonly IScheduledJobLogRepository _jobLogRepo;
    private readonly IScheduledJobExecutor _executor;

    /// <summary>
    /// Provides existing jobs from Optimizely.
    /// </summary>
    public TKOptimizelyJobsSource(IScheduledJobRepository jobRepo, IScheduledJobLogRepository jobLogRepo, IScheduledJobExecutor executor)
    {
        _jobRepo = jobRepo;
        _jobLogRepo = jobLogRepo;
        _executor = executor;
    }

    /// <summary></summary>
    public async Task<List<TKJobDefinition>> GetJobDefinitionsAsync()
    {
        List<TKJobDefinition> defs = new();
        var jobs = _jobRepo.List();
        var plugIns = GetIncludedJobPlugins();
        foreach (var plugin in plugIns)
        {
            var job = jobs.FirstOrDefault(j => j.TypeName == plugin.TypeName && j.AssemblyName == plugin.AssemblyName);
            if (job == null) continue;

            var pluginAttr = plugin.GetAttribute<ScheduledPlugInAttribute>();
            var jobAttr = plugin.PlugInType.GetCustomAttribute<TKJobAttribute>(true);
            var lastLog = (await _jobLogRepo.GetAsync(job.ID, 0, 1)).PagedResult.FirstOrDefault();
            defs.Add(new TKJobDefinition
            {
                Id = CreateJobId(plugin),
                Name = pluginAttr.DisplayName,
                Description = pluginAttr.Description,
                AllowedAccessRoles = null,
                GroupName = (job?.TypeName?.ToLower() == "episerver" == true
                            || job?.TypeName?.ToLower()?.StartsWith("episerver.") == true
                            || job?.TypeName?.ToLower() == "optimizely" == true
                            || job?.TypeName?.ToLower()?.StartsWith("optimizely.") == true)
                    ? "Default Optimizely Jobs"
                    : null,
                SupportsStart = true,
                SupportsStop = job?.IsStoppable == true,
                CustomParametersType = jobAttr?.CustomParametersType,
                CustomParameters = TKCustomPropertyAttribute.CreateInputConfigs(jobAttr?.CustomParametersType)
            });
        }
        return defs;
    }

    /// <summary></summary>
    public async Task<TKPagedJobLogItems> GetJobLogItemsPaged(string jobId, int pageIndex, int pageSize)
    {
        var plugin = GetIncludedJobPlugins().FirstOrDefault(x => CreateJobId(x) == jobId);
        if (plugin == null) return new TKPagedJobLogItems();
        var job = _jobRepo.List().FirstOrDefault(j => j.TypeName == plugin.TypeName && j.AssemblyName == plugin.AssemblyName);
        if (job == null) return new TKPagedJobLogItems();

        var logItems = await _jobLogRepo.GetAsync(job.ID, pageIndex * pageSize, pageSize);
        var items = logItems.PagedResult
            .Select(x =>
            {
                return new TKJobLogItem()
                {
                    Status = Create(x.Status),
                    Summary = x.Message,
                    Timestamp = x.CompletedUtc.ToLocalTime()
                };
            })
            .ToList();
        return new TKPagedJobLogItems
        {
            TotalCount = logItems.TotalCount,
            Items = items
        };
    }

    /// <summary></summary>
    public async Task<TKJobStatus> GetJobStatusAsync(string id)
    {
        List<TKJobStatus> statuses = new();
        var jobs = _jobRepo.List();
        var plugin = GetIncludedJobPlugins().FirstOrDefault(x => CreateJobId(x) == id);
        var job = jobs.FirstOrDefault(j => j.TypeName == plugin?.TypeName && j.AssemblyName == plugin?.AssemblyName);
        return await CreateStatus(plugin, job);
    }

    /// <summary></summary>
    public async Task<List<TKJobStatus>> GetJobStatusesAsync()
    {
        List<TKJobStatus> statuses = new();
        var jobs = _jobRepo.List();
        var plugIns = GetIncludedJobPlugins();
        foreach (var plugin in plugIns)
        {
            var job = jobs.FirstOrDefault(j => j.TypeName == plugin.TypeName && j.AssemblyName == plugin.AssemblyName);
            if (job == null) continue;
            var status = await CreateStatus(plugin, job);
            statuses.Add(status);
        }
        return statuses;
    }

    /// <summary></summary>
    public Task<TKJobStartResult> StartJobAsync(string jobId, TKJobStartCustomParameters parameters)
    {
        if (parameters.CustomParametersType != null && parameters.CustomParametersRaw?.Count > 0)
        {
            TKOptimizelyJobUtil.StoreDDSModelFor(jobId, new TKOptimizelyJobParametersDDSModel
            {
                ParametersTypeName = parameters.CustomParametersType.FullName,
                Values = parameters.CustomParametersRaw
            });
        }

        var jobs = _jobRepo.List();
        var plugin = GetIncludedJobPlugins()
            .FirstOrDefault(x => CreateJobId(x) == jobId);
        var job = jobs.FirstOrDefault(j => j.TypeName == plugin?.TypeName && j.AssemblyName == plugin?.AssemblyName);

        if (job == null) return Task.FromResult(new TKJobStartResult { Success = false, Message = "Job instance to start was not found." });
        _executor.StartAsync(job, new JobExecutionOptions
        {
            Trigger = ScheduledJobTrigger.User
        });
        return Task.FromResult(new TKJobStartResult { Success = true, Message = "Job was started." });
    }

    /// <summary></summary>
    public Task<TKJobStopResult> StopJobAsync(string jobId)
    {
        var jobs = _jobRepo.List();
        var plugin = GetIncludedJobPlugins()
            .FirstOrDefault(x => CreateJobId(x) == jobId);
        var job = jobs.FirstOrDefault(j => j.TypeName == plugin?.TypeName && j.AssemblyName == plugin?.AssemblyName);
        if (job == null) return Task.FromResult(new TKJobStopResult { Success = false, Message = "Job instance was not found." });

        _executor.Cancel(job.ID);
        return Task.FromResult(new TKJobStopResult { Success = true, Message = "Job was requested stopped." });
    }

    private PlugInDescriptor[] GetIncludedJobPlugins()
        => PlugInLocator.Search(new ScheduledPlugInAttribute())
        .Where(x => JobFilter == null || JobFilter(x))
        .ToArray();

    private TKJobHistoryStatus? Create(ScheduledJobExecutionStatus? x)
    {
        TKJobHistoryStatus? status = null;
        if (x == ScheduledJobExecutionStatus.Succeeded) status = TKJobHistoryStatus.Success;
        else if (x == ScheduledJobExecutionStatus.Cancelled) status = TKJobHistoryStatus.Cancelled;
        else if (x == ScheduledJobExecutionStatus.Aborted) status = TKJobHistoryStatus.Cancelled;
        else if (x == ScheduledJobExecutionStatus.Failed) status = TKJobHistoryStatus.Error;
        return status;
    }

    private string CreateJobId(PlugInDescriptor plugin) => TKJobsUtils.CreateJobId(plugin.PlugInType);

    private async Task<TKJobStatus> CreateStatus(PlugInDescriptor plugin, ScheduledJob job)
    {
        if (job == null) return new TKJobStatus
        {
            JobId = "[unknown]",
            Status = TKJobHistoryStatus.Warning
        };

        var lastLog = (await _jobLogRepo.GetAsync(job.ID, 0, 1)).PagedResult.FirstOrDefault();
        var isRunning = job.IsRunning && job.SecondsAfterLastPing < 30;
        var startedAt = ToDateTimeOffset(job.LastExecution);
        var endedAt = ToDateTimeOffset(lastLog?.CompletedUtc.ToLocalTime());

        var status = new TKJobStatus
        {
            JobId = CreateJobId(plugin),
            StartedAt = startedAt,
            EndedAt = endedAt,
            IsEnabled = job.IsEnabled,
            IsRunning = isRunning,
            Summary = job.CurrentStatusMessage ?? job.LastExecutionMessage,
            NextExecutionScheduledAt = ToDateTimeOffset(job.NextExecution),
            Status = Create(job?.LastExecutionStatus)
        };
        return status;
    }

    private static readonly DateTime _lowestDateTime = new(2, 1, 1);
    private static DateTimeOffset? ToDateTimeOffset(DateTime? dateTime)
        => dateTime < _lowestDateTime ? null : dateTime;
}

/// <summary>
/// Util for TK jobs used in Opti.
/// </summary>
public static class TKOptimizelyJobUtil
{
    /// <summary>
    /// Gets job parameters, or null if not set.
    /// <para>After this method is called, the stored parameters are removed if any.</para>
    /// </summary>
    /// <typeparam name="TJob">Type of the epi job.</typeparam>
    /// <typeparam name="TParameters">Type of your parameters class.</typeparam>
    public static TParameters GetJobParameters<TJob, TParameters>()
        where TParameters : class, new()
    {
        var jobId = TKJobsUtils.CreateJobId<TJob>();
        var ddsModel = GetDDSModelFor(jobId);
        if (ddsModel?.IsConsumed != false) return null;

        // Update model with consumed state
        ddsModel.IsConsumed = true;
        StoreDDSModelFor(jobId, ddsModel);

        // Create parameters instance
        var parametersRaw = ddsModel?.Values;
        return TKValueConversionUtils.ConvertInputModel<TParameters>(parametersRaw);
    }

    private static TKOptimizelyJobParametersDDSModel GetDDSModelFor(string jobId)
    {
        var store = typeof(TKOptimizelyJobParametersDDSModel).GetOrCreateStore();
        return store.Items<TKOptimizelyJobParametersDDSModel>().FirstOrDefault(x => x.JobId == jobId);
    }

    internal static void StoreDDSModelFor(string jobId, TKOptimizelyJobParametersDDSModel model)
    {
        var store = typeof(TKOptimizelyJobParametersDDSModel).GetOrCreateStore();
        var existingData = store.Items<TKOptimizelyJobParametersDDSModel>().FirstOrDefault(x => x.JobId == jobId);
        if (existingData != null)
        {
            model.Id = existingData.Id;
        }
        store.Save(model);
    }
}

/// <summary>
/// Model stored in DDS for job custom parameters.
/// </summary>
[EPiServerDataStore(AutomaticallyCreateStore = false, AutomaticallyRemapStore = true)]
public class TKOptimizelyJobParametersDDSModel
{
    /// <summary>
    /// Epi generated id.
    /// </summary>
    public Identity Id { get; set; }

    /// <summary></summary>
    public string JobId { get; set; }

    /// <summary></summary>
    public DateTimeOffset ConsumedAt { get; set; }

    /// <summary></summary>
    public bool IsConsumed { get; set; }

    /// <summary></summary>
    public string ParametersTypeName { get; set; }

    /// <summary></summary>
    public Dictionary<string, string> Values { get; set; }
}
