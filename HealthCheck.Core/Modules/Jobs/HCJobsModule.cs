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
            None = 0
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

            var result = await Options.Service.GetPagedHistoryAsync(model.JobId, model.PageIndex, model.PageSize);
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
        public async Task<List<HCJobHistoryEntryViewModel>> GetLatestHistoryPerJobId(HealthCheckModuleContext context)
        {
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
            var defs = await GetDefinitionsRequestCanAccessAsync(context);
            var allowedSourceIds = new HashSet<string>(defs.Select(x => x.SourceId));

            var result = await Options.Service.GetHistoryDetailAsync(model.Id);
            if (!allowedSourceIds.Contains(result.SourceId)) return null;

            return Create(result);
        }

        /// <summary></summary>
        [HealthCheckModuleMethod]
        public async Task<HCJobStartResultViewModel> StartJob(HealthCheckModuleContext context, HCJobsStartJobRequestModel model)
        {
            var defs = await GetDefinitionsRequestCanAccessAsync(context);
            var allowedSourceIds = new HashSet<string>(defs.Select(x => x.SourceId));
            if (!allowedSourceIds.Contains(model.SourceId)) return new HCJobStartResultViewModel { Message = "Job not found." };

            var parameters = "todo";
            var result = await Options.Service.StartJobAsync(model.SourceId, model.JobId, parameters);
            return Create(result);
        }

        /// <summary></summary>
        [HealthCheckModuleMethod]
        public async Task<HCJobStopResultViewModel> StopJob(HealthCheckModuleContext context, HCJobsStopJobRequestModel model)
        {
            var defs = await GetDefinitionsRequestCanAccessAsync(context);
            var allowedSourceIds = new HashSet<string>(defs.Select(x => x.SourceId));
            if (!allowedSourceIds.Contains(model.SourceId)) return new HCJobStopResultViewModel { Message = "Job not found." };

            var result = await Options.Service.StopJobAsync(model.SourceId, model.JobId);
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
                LastRunWasSuccessful = status.LastRunWasSuccessful,
                NextExecutionScheduledAt = status.NextExecutionScheduledAt,
                SourceId = status.SourceId,
                StartedAt = status.StartedAt,
                Status = status.Status
            };
        }

        private HCJobHistoryDetailEntryViewModel Create(HCJobHistoryDetailEntry result)
        {
            return new HCJobHistoryDetailEntryViewModel
            {
                Id = result.Id,
                SourceId = result.SourceId,
                Data = result.Data
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
                Timestamp = x.Timestamp
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
