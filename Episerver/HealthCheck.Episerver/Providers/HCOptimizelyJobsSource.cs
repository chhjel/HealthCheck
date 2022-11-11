using EPiServer.DataAbstraction;
using EPiServer.PlugIn;
using EPiServer.Scheduler;
using HealthCheck.Core.Modules.Jobs.Abstractions;
using HealthCheck.Core.Modules.Jobs.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HealthCheck.Episerver.Providers
{
    /// <summary>
    /// Provides existing jobs from Optimizely.
    /// </summary>
    public class HCOptimizelyJobsSource : IHCJobsSource
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
        public HCOptimizelyJobsSource(IScheduledJobRepository jobRepo, IScheduledJobLogRepository jobLogRepo, IScheduledJobExecutor executor)
        {
            _jobRepo = jobRepo;
            _jobLogRepo = jobLogRepo;
            _executor = executor;
        }

        /// <summary></summary>
        public async Task<List<HCJobDefinition>> GetJobDefinitionsAsync()
        {
            List<HCJobDefinition> defs = new();
            var jobs = _jobRepo.List();
            var plugIns = GetIncludedJobPlugins();
            foreach (var plugin in plugIns)
            {
                var job = jobs.FirstOrDefault(j => j.TypeName == plugin.TypeName && j.AssemblyName == plugin.AssemblyName);
                var attr = plugin.GetAttribute<ScheduledPlugInAttribute>();
                var lastLog = (await _jobLogRepo.GetAsync(job.ID, 0, 1)).PagedResult.FirstOrDefault();
                defs.Add(new HCJobDefinition
                {
                    Id = CreateJobId(plugin),
                    Name = attr.DisplayName,
                    Description = attr.Description,
                    AllowedAccessRoles = null,
                    GroupName = (job?.TypeName?.ToLower() == "episerver" == true
                                || job?.TypeName?.ToLower()?.StartsWith("episerver.") == true
                                || job?.TypeName?.ToLower() == "optimizely" == true
                                || job?.TypeName?.ToLower()?.StartsWith("optimizely.") == true)
                        ? "Default Optimizely Jobs"
                        : null,
                    SupportsStart = true,
                    SupportsStop= job?.IsStoppable == true
                });
            }
            return defs;
        }

        /// <summary></summary>
        public async Task<HCPagedJobLogItems> GetJobLogItemsPaged(string jobId, int pageIndex, int pageSize)
        {
            var plugin = GetIncludedJobPlugins().FirstOrDefault(x => CreateJobId(x) == jobId);
            if (plugin == null) return new HCPagedJobLogItems();
            var job = _jobRepo.List().FirstOrDefault(j => j.TypeName == plugin.TypeName && j.AssemblyName == plugin.AssemblyName);
            if (job == null) return new HCPagedJobLogItems();

            var logItems = await _jobLogRepo.GetAsync(job.ID, pageIndex * pageSize, pageSize);
            var items = logItems.PagedResult
                .Select(x => {
                    return new HCJobLogItem()
                    {
                        Status = Create(x.Status),
                        Summary = x.Message,
                        Timestamp = x.CompletedUtc.ToLocalTime()
                    };
                })
                .ToList();
            return new HCPagedJobLogItems
            {
                TotalCount = logItems.TotalCount,
                Items = items
            };
        }

        /// <summary></summary>
        public async Task<HCJobStatus> GetJobStatusAsync(string id)
        {
            List<HCJobStatus> statuses = new();
            var jobs = _jobRepo.List();
            var plugin = GetIncludedJobPlugins().FirstOrDefault(x => CreateJobId(x) == id);
            var job = jobs.FirstOrDefault(j => j.TypeName == plugin?.TypeName && j.AssemblyName == plugin?.AssemblyName);
            var attr = plugin.GetAttribute<ScheduledPlugInAttribute>();
            var lastLog = (await _jobLogRepo.GetAsync(job.ID, 0, 1)).PagedResult.FirstOrDefault();
            var status = new HCJobStatus
            {
                JobId = CreateJobId(plugin),
                StartedAt = job.LastExecution,
                EndedAt = lastLog.CompletedUtc.ToLocalTime() + lastLog.Duration,
                IsEnabled = job.IsEnabled,
                IsRunning = job.IsRunning,
                Summary = job.CurrentStatusMessage ?? job.LastExecutionMessage,
                NextExecutionScheduledAt = job.NextExecution,
                Status = Create(job?.LastExecutionStatus)
            };
            return status;
        }

        /// <summary></summary>
        public async Task<List<HCJobStatus>> GetJobStatusesAsync()
        {
            List<HCJobStatus> statuses = new();
            var jobs = _jobRepo.List();
            var plugIns = GetIncludedJobPlugins();
            foreach (var plugin in plugIns)
            {
                var job = jobs.FirstOrDefault(j => j.TypeName == plugin.TypeName && j.AssemblyName == plugin.AssemblyName);
                if (job == null) continue;
                var attr = plugin.GetAttribute<ScheduledPlugInAttribute>();
                var lastLog = (await _jobLogRepo.GetAsync(job.ID, 0, 1)).PagedResult.FirstOrDefault();
                var status = new HCJobStatus
                {
                    JobId = CreateJobId(plugin),
                    StartedAt = ToDateTimeOffset(job.LastExecution),
                    EndedAt = ToDateTimeOffset(lastLog?.CompletedUtc.ToLocalTime() + lastLog?.Duration),
                    IsEnabled = job.IsEnabled,
                    IsRunning = job.IsRunning,
                    Summary = job.CurrentStatusMessage ?? job.LastExecutionMessage,
                    NextExecutionScheduledAt = ToDateTimeOffset(job.NextExecution),
                    Status = Create(job?.LastExecutionStatus)
                };

                statuses.Add(status);
            }
            return statuses;
        }

        /// <summary></summary>
        public Task<HCJobStartResult> StartJobAsync(string jobId, object parameters)
        {
            // ToDo: if parameters is not null:
            // 1. save parameters to DDS, 2. start job, 3. job loads parameters
            // - in job defs, include parameters. Set from type on attribute.

            var jobs = _jobRepo.List();
            var plugin = GetIncludedJobPlugins()
                .FirstOrDefault(x => CreateJobId(x) == jobId);
            var job = jobs.FirstOrDefault(j => j.TypeName == plugin?.TypeName && j.AssemblyName == plugin?.AssemblyName);

            if (job == null) return Task.FromResult(new HCJobStartResult { Success = false, Message = "Job instance to start was not found." });
            _executor.StartAsync(job, new JobExecutionOptions
            {
                Trigger = ScheduledJobTrigger.User
            });
            return Task.FromResult(new HCJobStartResult { Success = true, Message = "Job was started." });
        }

        /// <summary></summary>
        public Task<HCJobStopResult> StopJobAsync(string jobId)
        {
            var jobs = _jobRepo.List();
            var plugin = GetIncludedJobPlugins()
                .FirstOrDefault(x => CreateJobId(x) == jobId);
            var job = jobs.FirstOrDefault(j => j.TypeName == plugin?.TypeName && j.AssemblyName == plugin?.AssemblyName);
            if (job == null) return Task.FromResult(new HCJobStopResult { Success = false, Message = "Job instance was not found." });

            _executor.Cancel(job.ID);
            return Task.FromResult(new HCJobStopResult { Success = true, Message = "Job was requested stopped." });
        }

        private PlugInDescriptor[] GetIncludedJobPlugins()
            => PlugInLocator.Search(new ScheduledPlugInAttribute())
            .Where(x => JobFilter == null || JobFilter(x))
            .ToArray();

        private HCJobHistoryStatus? Create(ScheduledJobExecutionStatus? x)
        {
            HCJobHistoryStatus? status = null;
            if (x == ScheduledJobExecutionStatus.Succeeded) status = HCJobHistoryStatus.Success;
            else if (x == ScheduledJobExecutionStatus.Cancelled) status = HCJobHistoryStatus.Cancelled;
            else if (x == ScheduledJobExecutionStatus.Aborted) status = HCJobHistoryStatus.Cancelled;
            else if (x == ScheduledJobExecutionStatus.Failed) status = HCJobHistoryStatus.Error;
            return status;
        }

        private string CreateJobId(PlugInDescriptor plugin) => $"{plugin.TypeName}_{plugin.AssemblyName}";

        private static readonly DateTime _lowestDateTime = new(2, 1, 1);
        private static DateTimeOffset? ToDateTimeOffset(DateTime? dateTime)
            => dateTime < _lowestDateTime ? null : dateTime;
    }
}
