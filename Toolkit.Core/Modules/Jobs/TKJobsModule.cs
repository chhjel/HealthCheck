using QoDL.Toolkit.Core.Abstractions.Modules;
using QoDL.Toolkit.Core.Modules.Jobs.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QoDL.Toolkit.Core.Modules.Jobs;

/// <summary>
/// 
/// </summary>
public class TKJobsModule : ToolkitModuleBase<TKJobsModule.AccessOption>
{
    private TKJobsModuleOptions Options { get; }

    /// <summary>
    /// 
    /// </summary>
    public TKJobsModule(TKJobsModuleOptions options)
    {
        Options = options;
    }

    /// <summary>
    /// Check options object for issues.
    /// </summary>
    public override IEnumerable<string> Validate()
    {
        var issues = new List<string>();
        if (Options.Service == null) issues.Add("Options.Service must be set.");
        return issues;
    }

    /// <summary>
    /// Get frontend options for this module.
    /// </summary>
    public override object GetFrontendOptionsObject(ToolkitModuleContext context) => null;

    /// <summary>
    /// Get config for this module.
    /// </summary>
    public override IToolkitModuleConfig GetModuleConfig(ToolkitModuleContext context) => new TKJobsModuleConfig();
    
    /// <summary>
    /// Different access options for this module.
    /// </summary>
    [Flags]
    public enum AccessOption
    {
        /// <summary>Does nothing.</summary>
        None = 0,
        /// <summary>Allows starting jobs that support it.</summary>
        StartJob = 1,
        /// <summary>Allows stopping jobs that support it.</summary>
        StopJob = 2,
        /// <summary>Allows viewing the history of a job.</summary>
        ViewJobHistory = 4,
        /// <summary>Allows viewing the detailed results of a job.</summary>
        ViewJobHistoryDetails = 8,
        /// <summary>Allows deleting history of jobs the user has access to.</summary>
        DeleteHistory = 16,
        /// <summary>Allows deleting all job history, including of those the user does not have access to.</summary>
        DeleteAllHistory = 32,
        /// <summary>Allows starting jobs with custom parameters for those that support it.</summary>
        StartJobWithCustomParameters = 64,
    }

    #region Invokable methods
    /// <summary></summary>
    [ToolkitModuleMethod]
    public async Task<List<TKJobDefinitionWithSourceViewModel>> GetJobDefinitions(ToolkitModuleContext context)
    {
        var defs = await GetDefinitionsRequestCanAccessAsync(context);
        var models = defs.Select(x => Create(x)).ToList();
        return models;
    }

    /// <summary></summary>
    [ToolkitModuleMethod]
    public async Task<TKPagedJobHistoryEntryViewModel> GetPagedHistory(ToolkitModuleContext context, TKJobsGetPagedHistoryRequestModel model)
    {
        var defs = await GetDefinitionsRequestCanAccessAsync(context);
        var allowedSourceIds = new HashSet<string>(defs.Select(x => x.SourceId));
        if (!allowedSourceIds.Contains(model.SourceId)) return new TKPagedJobHistoryEntryViewModel();

        var result = await Options.Service.GetPagedHistoryAsync(model.SourceId, model.JobId, model.PageIndex, model.PageSize);
        var models = result.Items
            .Where(x => allowedSourceIds.Contains(x.SourceId))
            .Select(x => Create(x))
            .ToList();

        return new TKPagedJobHistoryEntryViewModel
        {
            Items = models,
            TotalCount = result.TotalCount
        };
    }

    /// <summary></summary>
    [ToolkitModuleMethod]
    public async Task<TKPagedJobHistoryEntryViewModel> GetPagedLogItems(ToolkitModuleContext context, TKJobsGetPagedHistoryRequestModel model)
    {
        var defs = await GetDefinitionsRequestCanAccessAsync(context);
        var allowedSourceIds = new HashSet<string>(defs.Select(x => x.SourceId));
        if (!allowedSourceIds.Contains(model.SourceId)) return new TKPagedJobHistoryEntryViewModel();

        var result = await Options.Service.GetPagedJobLogItemsAsync(model.SourceId, model.JobId, model.PageIndex, model.PageSize);
        var models = result.Items
            .Select(x => Create(x, model.SourceId, model.JobId))
            .ToList();

        return new TKPagedJobHistoryEntryViewModel
        {
            Items = models,
            TotalCount = result.TotalCount
        };
    }

    /// <summary></summary>
    [ToolkitModuleMethod]
    public async Task<List<TKJobHistoryEntryViewModel>> GetLatestHistoryPerJobId(ToolkitModuleContext context)
    {
        if (!context.HasAccess(AccessOption.ViewJobHistory)) return new List<TKJobHistoryEntryViewModel>();

        var defs = await GetDefinitionsRequestCanAccessAsync(context);
        var allowedSourceIds = new HashSet<string>(defs.Select(x => x.SourceId));

        var result = await Options.Service.GetLatestHistoryPerJobIdAsync();
        var models = result
            .Where(x => allowedSourceIds.Contains(x.SourceId))
            .Select(x => Create(x))
            .ToList();

        return models;
    }

    /// <summary></summary>
    [ToolkitModuleMethod]
    public async Task<TKJobHistoryDetailEntryViewModel> GetHistoryDetail(ToolkitModuleContext context, TKJobsGetHistoryDetailRequestModel model)
    {
        if (!context.HasAccess(AccessOption.ViewJobHistoryDetails)) return null;

        var defs = await GetDefinitionsRequestCanAccessAsync(context);
        var allowedSourceIds = new HashSet<string>(defs.Select(x => x.SourceId));

        var result = await Options.Service.GetHistoryDetailAsync(model.Id);
        if (result == null) return null;
        if (!allowedSourceIds.Contains(result.SourceId)) return new TKJobHistoryDetailEntryViewModel
        {
            Id = model.Id,
            DataIsHtml = true,
            Data = $"<b>Not found</b>"
        };

        return Create(result);
    }

    /// <summary></summary>
    [ToolkitModuleMethod]
    public async Task<TKJobStartResultViewModel> StartJob(ToolkitModuleContext context, TKJobsStartJobRequestModel model)
    {
        if (!context.HasAccess(AccessOption.StartJob)) return new TKJobStartResultViewModel { Message = "You do not have access to start this job." };
        if (!context.HasAccess(AccessOption.StartJobWithCustomParameters))
        {
            model.Parameters = null;
        }

        var defs = await GetDefinitionsRequestCanAccessAsync(context);
        var allowedSourceIds = new HashSet<string>(defs.Select(x => x.SourceId));
        if (!allowedSourceIds.Contains(model.SourceId)) return new TKJobStartResultViewModel { Message = "Job not found." };

        var result = await Options.Service.StartJobAsync(model.SourceId, model.JobId, model.Parameters);
        context.AddAuditEvent(action: "Job started", subject: $"\"{model.JobId}\"")
            .AddDetail("Result", result.Message);
        return Create(result);
    }

    /// <summary></summary>
    [ToolkitModuleMethod]
    public async Task<TKJobStopResultViewModel> StopJob(ToolkitModuleContext context, TKJobsStopJobRequestModel model)
    {
        if (!context.HasAccess(AccessOption.StopJob)) return new TKJobStopResultViewModel { Message = "You do not have access to stop this job." };

        var defs = await GetDefinitionsRequestCanAccessAsync(context);
        var allowedSourceIds = new HashSet<string>(defs.Select(x => x.SourceId));
        if (!allowedSourceIds.Contains(model.SourceId)) return new TKJobStopResultViewModel { Message = "Job not found." };

        var result = await Options.Service.StopJobAsync(model.SourceId, model.JobId);
        context.AddAuditEvent(action: "Job stopped", subject: $"\"{model.JobId}\"")
            .AddDetail("Result", result.Message);
        return Create(result);
    }

    /// <summary></summary>
    [ToolkitModuleMethod]
    public async Task<TKJobStatusViewModel> GetJobStatus(ToolkitModuleContext context, TKJobsGetJobStatusRequestModel model)
    {
        var defs = await GetDefinitionsRequestCanAccessAsync(context);
        var allowedSourceIds = new HashSet<string>(defs.Select(x => x.SourceId));
        if (!allowedSourceIds.Contains(model.SourceId)) return null;

        var status = await Options.Service.GetJobStatusAsync(model.SourceId, model.JobId);
        return Create(status);
    }

    /// <summary></summary>
    [ToolkitModuleMethod]
    public async Task<List<TKJobStatusViewModel>> GetJobStatuses(ToolkitModuleContext context)
    {
        var defs = await GetDefinitionsRequestCanAccessAsync(context);
        var allowedSourceIds = new HashSet<string>(defs.Select(x => x.SourceId));

        var statuses = await Options.Service.GetJobStatusesAsync();
        var models = statuses
            .Select(x => Create(x))
            .Where(x => allowedSourceIds.Contains(x.SourceId))
            .ToList();
        return models;
    }

    /// <summary></summary>
    [ToolkitModuleMethod(AccessOption.DeleteAllHistory)]
    public async Task<TKJobSimpleResult> DeleteAllHistory() => await Options.Service.DeleteAllHistoryAsync();

    /// <summary></summary>
    [ToolkitModuleMethod(AccessOption.DeleteHistory)]
    public async Task<TKJobSimpleResult> DeleteHistoryItem(ToolkitModuleContext context, TKJobDeleteHistoryItemRequestModel model)
    {
        var defs = await GetDefinitionsRequestCanAccessAsync(context);
        var allowedSourceIds = new HashSet<string>(defs.Select(x => x.SourceId));
        return await Options.Service.DeleteHistoryItemAsync(model.Id, item => allowedSourceIds.Contains(item.SourceId));
    }

    /// <summary></summary>
    [ToolkitModuleMethod(AccessOption.DeleteHistory)]
    public async Task<TKJobSimpleResult> DeleteAllHistoryForJob(ToolkitModuleContext context, TKJobDeleteAllHistoryForJobRequestModel model)
    {
        var defs = await GetDefinitionsRequestCanAccessAsync(context);
        var allowedSourceIds = new HashSet<string>(defs.Select(x => x.SourceId));
        if (!allowedSourceIds.Contains(model.SourceId))
            return TKJobSimpleResult.CreateError("You do not have access to delete this history.");

        return await Options.Service.DeleteAllHistoryForJobAsync(model.SourceId, model.JobId);
    }

    // ToDo:
    // Util:
    //  InsertJobHistory(data.., detail..)
    #endregion

    #region Helpers
    private async Task<IEnumerable<TKJobDefinitionWithSource>> GetDefinitionsRequestCanAccessAsync(ToolkitModuleContext context)
    {
        var defs = (await Options.Service.GetJobDefinitions()) ?? new List<TKJobDefinitionWithSource>();
        return defs.Where(x =>
            (x?.Definition?.AllowedAccessRoles == null) || context.HasRoleAccessObj(x?.Definition?.AllowedAccessRoles)
            && (context.CurrentModuleCategoryAccess?.Any() != true || context.CurrentModuleCategoryAccess.Any(c => x.Definition?.Categories?.Contains(c) == true))
        );
    }
    #endregion

    #region Factories
    private TKJobStartResultViewModel Create(TKJobStartResult result)
    {
        return new TKJobStartResultViewModel
        {
            Success = result.Success,
            Message = result.Message
        };
    }

    private TKJobStopResultViewModel Create(TKJobStopResult result)
    {
        return new TKJobStopResultViewModel
        {
            Success = result.Success,
            Message = result.Message
        };
    }

    private TKJobStatusViewModel Create(TKJobStatus status)
    {
        return new TKJobStatusViewModel
        {
            EndedAt = status.EndedAt,
            IsEnabled = status.IsEnabled,
            IsRunning = status.IsRunning,
            JobId = status.JobId,
            Status = status.Status,
            NextExecutionScheduledAt = status.NextExecutionScheduledAt,
            SourceId = status.SourceId,
            StartedAt = status.StartedAt?.ToLocalTime(),
            Summary = status.Summary
        };
    }

    private TKJobHistoryDetailEntryViewModel Create(TKJobHistoryDetailEntry result)
    {
        return new TKJobHistoryDetailEntryViewModel
        {
            Id = result.Id,
            SourceId = result.SourceId,
            JobId = result.JobId,
            Data = result.Data,
            DataIsHtml = result.DataIsHtml
        };
    }

    private TKJobHistoryEntryViewModel Create(TKJobHistoryEntry x)
    {
        return new TKJobHistoryEntryViewModel
        {
            Id = x.Id,
            JobId = x.JobId,
            DetailId = x.DetailId,
            SourceId = x.SourceId,
            Summary = x.Summary,
            Status = x.Status,
            EndedAt = x.EndedAt.ToLocalTime(),
            StartedAt = x.StartedAt?.ToLocalTime()
        };
    }

    private TKJobHistoryEntryViewModel Create(TKJobLogItem x, string sourceId, string jobId)
    {
        return new TKJobHistoryEntryViewModel
        {
            Id = Guid.Empty,
            JobId = jobId,
            SourceId = sourceId,
            Summary = x.Summary,
            Status = x.Status ?? TKJobHistoryStatus.Warning,
            EndedAt = x.Timestamp.ToLocalTime()
        };
    }

    private TKJobDefinitionWithSourceViewModel Create(TKJobDefinitionWithSource x)
    {
        return new TKJobDefinitionWithSourceViewModel
        {
            SourceId = x.SourceId,
            Definition = Create(x.Definition)
        };
    }

    private TKJobDefinitionViewModel Create(TKJobDefinition x)
    {
        return new TKJobDefinitionViewModel
        {
            Id = x.Id,
            Name = x.Name,
            Categories = x.Categories ?? new(),
            Description = x.Description,
            GroupName = x.GroupName,
            SupportsStart = x.SupportsStart,
            SupportsStop = x.SupportsStop,
            CustomParameters = x.CustomParameters,
            HasCustomParameters = x.CustomParameters?.Count > 0
        };
    }
    #endregion
}
