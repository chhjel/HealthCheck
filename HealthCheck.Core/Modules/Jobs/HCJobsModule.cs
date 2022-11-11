using HealthCheck.Core.Abstractions.Modules;
using HealthCheck.Core.Modules.Jobs.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HealthCheck.Core.Modules.Jobs
{
    /// <summary>
    /// 
    /// </summary>
    public class HCJobsModule : HealthCheckModuleBase<HCJobsModule.AccessOption>
    {
        private HCJobsModuleOptions Options { get; }

        /// <summary>
        /// 
        /// </summary>
        public HCJobsModule(HCJobsModuleOptions options)
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
        public override object GetFrontendOptionsObject(HealthCheckModuleContext context) => null;

        /// <summary>
        /// Get config for this module.
        /// </summary>
        public override IHealthCheckModuleConfig GetModuleConfig(HealthCheckModuleContext context) => new HCJobsModuleConfig();
        
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
        }

        #region Invokable methods
        /// <summary></summary>
        [HealthCheckModuleMethod]
        public async Task<List<HCJobDefinitionWithSourceViewModel>> GetJobDefinitions(HealthCheckModuleContext context)
        {
            var defs = await GetDefinitionsRequestCanAccessAsync(context);
            var models = defs.Select(x => Create(x)).ToList();
            return models;
        }

        /// <summary></summary>
        [HealthCheckModuleMethod]
        public async Task<HCPagedJobHistoryEntryViewModel> GetPagedHistory(HealthCheckModuleContext context, HCJobsGetPagedHistoryRequestModel model)
        {
            var defs = await GetDefinitionsRequestCanAccessAsync(context);
            var allowedSourceIds = new HashSet<string>(defs.Select(x => x.SourceId));
            if (!allowedSourceIds.Contains(model.SourceId)) return new HCPagedJobHistoryEntryViewModel();

            var result = await Options.Service.GetPagedHistoryAsync(model.SourceId, model.JobId, model.PageIndex, model.PageSize);
            var models = result.Items
                .Where(x => allowedSourceIds.Contains(x.SourceId))
                .Select(x => Create(x))
                .ToList();

            return new HCPagedJobHistoryEntryViewModel
            {
                Items = models,
                TotalCount = result.TotalCount
            };
        }

        /// <summary></summary>
        [HealthCheckModuleMethod]
        public async Task<HCPagedJobHistoryEntryViewModel> GetPagedLogItems(HealthCheckModuleContext context, HCJobsGetPagedHistoryRequestModel model)
        {
            var defs = await GetDefinitionsRequestCanAccessAsync(context);
            var allowedSourceIds = new HashSet<string>(defs.Select(x => x.SourceId));
            if (!allowedSourceIds.Contains(model.SourceId)) return new HCPagedJobHistoryEntryViewModel();

            var result = await Options.Service.GetPagedJobLogItemsAsync(model.SourceId, model.JobId, model.PageIndex, model.PageSize);
            var models = result.Items
                .Select(x => Create(x, model.SourceId, model.JobId))
                .ToList();

            return new HCPagedJobHistoryEntryViewModel
            {
                Items = models,
                TotalCount = result.TotalCount
            };
        }

        /// <summary></summary>
        [HealthCheckModuleMethod]
        public async Task<List<HCJobHistoryEntryViewModel>> GetLatestHistoryPerJobId(HealthCheckModuleContext context)
        {
            if (!context.HasAccess(AccessOption.ViewJobHistory)) return new List<HCJobHistoryEntryViewModel>();

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
        [HealthCheckModuleMethod]
        public async Task<HCJobHistoryDetailEntryViewModel> GetHistoryDetail(HealthCheckModuleContext context, HCJobsGetHistoryDetailRequestModel model)
        {
            if (!context.HasAccess(AccessOption.ViewJobHistoryDetails)) return null;

            var defs = await GetDefinitionsRequestCanAccessAsync(context);
            var allowedSourceIds = new HashSet<string>(defs.Select(x => x.SourceId));

            var result = await Options.Service.GetHistoryDetailAsync(model.Id);
            if (result == null) return null;
            if (!allowedSourceIds.Contains(result.SourceId)) return new HCJobHistoryDetailEntryViewModel
            {
                Id = model.Id,
                DataIsHtml = true,
                Data = $"<b>Not found</b>"
            };

            return Create(result);
        }

        /// <summary></summary>
        [HealthCheckModuleMethod]
        public async Task<HCJobStartResultViewModel> StartJob(HealthCheckModuleContext context, HCJobsStartJobRequestModel model)
        {
            if (!context.HasAccess(AccessOption.StartJob)) return new HCJobStartResultViewModel { Message = "You do not have access to start this job." };

            var defs = await GetDefinitionsRequestCanAccessAsync(context);
            var allowedSourceIds = new HashSet<string>(defs.Select(x => x.SourceId));
            if (!allowedSourceIds.Contains(model.SourceId)) return new HCJobStartResultViewModel { Message = "Job not found." };

            var parameters = "todo";
            var result = await Options.Service.StartJobAsync(model.SourceId, model.JobId, parameters);
            context.AddAuditEvent(action: "Job started", subject: $"\"{model.JobId}\"")
                .AddDetail("Result", result.Message);
            return Create(result);
        }

        /// <summary></summary>
        [HealthCheckModuleMethod]
        public async Task<HCJobStopResultViewModel> StopJob(HealthCheckModuleContext context, HCJobsStopJobRequestModel model)
        {
            if (!context.HasAccess(AccessOption.StopJob)) return new HCJobStopResultViewModel { Message = "You do not have access to stop this job." };

            var defs = await GetDefinitionsRequestCanAccessAsync(context);
            var allowedSourceIds = new HashSet<string>(defs.Select(x => x.SourceId));
            if (!allowedSourceIds.Contains(model.SourceId)) return new HCJobStopResultViewModel { Message = "Job not found." };

            var result = await Options.Service.StopJobAsync(model.SourceId, model.JobId);
            context.AddAuditEvent(action: "Job stopped", subject: $"\"{model.JobId}\"")
                .AddDetail("Result", result.Message);
            return Create(result);
        }

        /// <summary></summary>
        [HealthCheckModuleMethod]
        public async Task<HCJobStatusViewModel> GetJobStatus(HealthCheckModuleContext context, HCJobsGetJobStatusRequestModel model)
        {
            var defs = await GetDefinitionsRequestCanAccessAsync(context);
            var allowedSourceIds = new HashSet<string>(defs.Select(x => x.SourceId));
            if (!allowedSourceIds.Contains(model.SourceId)) return null;

            var status = await Options.Service.GetJobStatusAsync(model.SourceId, model.JobId);
            return Create(status);
        }

        /// <summary></summary>
        [HealthCheckModuleMethod]
        public async Task<List<HCJobStatusViewModel>> GetJobStatuses(HealthCheckModuleContext context)
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
        [HealthCheckModuleMethod(AccessOption.DeleteAllHistory)]
        public async Task<HCJobSimpleResult> DeleteAllHistory() => await Options.Service.DeleteAllHistoryAsync();

        /// <summary></summary>
        [HealthCheckModuleMethod(AccessOption.DeleteHistory)]
        public async Task<HCJobSimpleResult> DeleteHistoryItem(HealthCheckModuleContext context, HCJobDeleteHistoryItemRequestModel model)
        {
            var defs = await GetDefinitionsRequestCanAccessAsync(context);
            var allowedSourceIds = new HashSet<string>(defs.Select(x => x.SourceId));
            return await Options.Service.DeleteHistoryItemAsync(model.Id, item => allowedSourceIds.Contains(item.SourceId));
        }

        /// <summary></summary>
        [HealthCheckModuleMethod(AccessOption.DeleteHistory)]
        public async Task<HCJobSimpleResult> DeleteAllHistoryForJob(HealthCheckModuleContext context, HCJobDeleteAllHistoryForJobRequestModel model)
        {
            var defs = await GetDefinitionsRequestCanAccessAsync(context);
            var allowedSourceIds = new HashSet<string>(defs.Select(x => x.SourceId));
            if (!allowedSourceIds.Contains(model.SourceId))
                return HCJobSimpleResult.CreateError("You do not have access to delete this history.");

            return await Options.Service.DeleteAllHistoryForJobAsync(model.SourceId, model.JobId);
        }

        // ToDo:
        // Util:
        //  InsertJobHistory(data.., detail..)
        #endregion

        #region Helpers
        private async Task<IEnumerable<HCJobDefinitionWithSource>> GetDefinitionsRequestCanAccessAsync(HealthCheckModuleContext context)
        {
            var defs = (await Options.Service.GetJobDefinitions()) ?? new List<HCJobDefinitionWithSource>();
            return defs.Where(x =>
                (x?.Definition?.AllowedAccessRoles == null) || context.HasRoleAccessObj(x?.Definition?.AllowedAccessRoles)
                && (context.CurrentModuleCategoryAccess?.Any() != true || context.CurrentModuleCategoryAccess.Any(c => x.Definition?.Categories?.Contains(c) == true))
            );
        }
        #endregion

        #region Factories
        private HCJobStartResultViewModel Create(HCJobStartResult result)
        {
            return new HCJobStartResultViewModel
            {
                Success = result.Success,
                Message = result.Message
            };
        }

        private HCJobStopResultViewModel Create(HCJobStopResult result)
        {
            return new HCJobStopResultViewModel
            {
                Success = result.Success,
                Message = result.Message
            };
        }

        private HCJobStatusViewModel Create(HCJobStatus status)
        {
            return new HCJobStatusViewModel
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

        private HCJobHistoryDetailEntryViewModel Create(HCJobHistoryDetailEntry result)
        {
            return new HCJobHistoryDetailEntryViewModel
            {
                Id = result.Id,
                SourceId = result.SourceId,
                JobId = result.JobId,
                Data = result.Data,
                DataIsHtml = result.DataIsHtml
            };
        }

        private HCJobHistoryEntryViewModel Create(HCJobHistoryEntry x)
        {
            return new HCJobHistoryEntryViewModel
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

        private HCJobHistoryEntryViewModel Create(HCJobLogItem x, string sourceId, string jobId)
        {
            return new HCJobHistoryEntryViewModel
            {
                Id = Guid.Empty,
                JobId = jobId,
                SourceId = sourceId,
                Summary = x.Summary,
                Status = x.Status ?? HCJobHistoryStatus.Warning,
                EndedAt = x.Timestamp.ToLocalTime()
            };
        }

        private HCJobDefinitionWithSourceViewModel Create(HCJobDefinitionWithSource x)
        {
            return new HCJobDefinitionWithSourceViewModel
            {
                SourceId = x.SourceId,
                Definition = Create(x.Definition)
            };
        }

        private HCJobDefinitionViewModel Create(HCJobDefinition x)
        {
            return new HCJobDefinitionViewModel
            {
                Id = x.Id,
                Name = x.Name,
                Categories = x.Categories ?? new(),
                Description = x.Description,
                GroupName = x.GroupName,
                SupportsStart = x.SupportsStart,
                SupportsStop = x.SupportsStop
            };
        }
        #endregion
    }
}
