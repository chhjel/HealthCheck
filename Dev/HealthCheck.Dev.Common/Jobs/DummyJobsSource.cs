using HealthCheck.Core.Modules.Jobs.Abstractions;
using HealthCheck.Core.Modules.Jobs.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HealthCheck.Dev.Common.Jobs
{
    public class DummyJobsSource : IHCJobsSource
    {
        public string Name { get; } = "Dummy Jobs";
        public string Description { get; } = "Dev job source.";
        private static Dictionary<string, HCJobStatus> _jobStatuses = new();

        public Task<List<HCJobDefinition>> GetJobDefinitionsAsync()
        {
            var jobs = Enumerable.Range(1, 8)
                .Select(x => new HCJobDefinition
                {
                    Id = x.ToString(),
                    Name = $"Job #{x}",
                    Description = $"Some description here for job #{x}",
                    AllowedAccessRoles = null,
                    GroupName = (x % 5 == 0) ? null : $"Group #{x % 2}",
                    SupportsStart = x == 0 || x == 1 || x > 3,
                    SupportsStop = x == 0 || x == 2|| x > 3,
                })
                .ToList();
            return Task.FromResult(jobs);
        }

        public Task<HCJobStatus> GetJobStatusAsync(string id)
            => _jobStatuses.TryGetValue(id, out var status)
                ? Task.FromResult(status)
                : Task.FromResult<HCJobStatus>(null);

        public Task<List<HCJobStatus>> GetJobStatusesAsync()
            => Task.FromResult(_jobStatuses.Values.ToList());

        public Task<HCJobStartResult> StartJobAsync(string jobId, object parameters)
        {
            if (!_jobStatuses.ContainsKey(jobId))
            {
                _jobStatuses[jobId] = new HCJobStatus
                {
                    JobId = jobId,
                    IsEnabled = true,
                    NextExecutionScheduledAt = DateTime.Now.AddMinutes(12)
                };
            }

            if (_jobStatuses[jobId].IsRunning)
            {
                return Task.FromResult(new HCJobStartResult { Message = "Job is already running." });
            }

            var status = _jobStatuses[jobId];
            status.StartedAt = DateTimeOffset.Now;
            status.IsRunning = true;
            status.Status = "Job started";
            Task.Run(async () =>
            {
                await Task.Delay(TimeSpan.FromSeconds(10));
                status.EndedAt = DateTimeOffset.Now;
                status.IsRunning = false;
                status.Status = "Finished";
            });

            return Task.FromResult(new HCJobStartResult { Success = true, Message = "Job was started." });
        }

        public Task<HCJobStopResult> StopJobAsync(string jobId)
        {
            if (!_jobStatuses.ContainsKey(jobId))
            {
                _jobStatuses[jobId] = new HCJobStatus
                {
                    JobId = jobId,
                    IsEnabled = true,
                    NextExecutionScheduledAt = DateTime.Now.AddMinutes(12)
                };
            }

            if (!_jobStatuses[jobId].IsRunning)
            {
                return Task.FromResult(new HCJobStopResult { Message = "Job is not running." });
            }

            var status = _jobStatuses[jobId];
            status.EndedAt = DateTimeOffset.Now;
            status.IsRunning = false;
            status.Status = "Job stopped";

            return Task.FromResult(new HCJobStopResult { Success = true, Message = "Job was stopped." });
        }
    }
}
