using HealthCheck.Core.Attributes;
using HealthCheck.Core.Extensions;
using HealthCheck.Core.Modules.Jobs;
using HealthCheck.Core.Modules.Jobs.Abstractions;
using HealthCheck.Core.Modules.Jobs.Models;
using HealthCheck.Core.Modules.Tests.Utils.HtmlPresets;
using Newtonsoft.Json;
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
                    Id = $"{typeof(DummyJob).FullName}_{typeof(DummyJob).Assembly.ShortName()}_{x}",
                    Name = $"Job #{x}",
                    Description = string.Concat(Enumerable.Repeat($"Some description here for job #{x} ", x)),
                    AllowedAccessRoles = null,
                    GroupName = (x % 5 == 0) ? null : $"Group #{x % 2}",
                    SupportsStart = x == 0 || x == 1 || x > 3,
                    SupportsStop = x == 0 || x == 2 || x > 3,
                    CustomParametersType = x % 2 == 0 ? typeof(DummyJobParameters) : null,
                    CustomParameters = x % 2 == 0 ? HCCustomPropertyAttribute.CreateInputConfigs(typeof(DummyJobParameters)) : null
                })
                .ToList();
            return Task.FromResult(jobs);
        }

        public Task<HCPagedJobLogItems> GetJobLogItemsPaged(string jobId, int pageIndex, int pageSize)
            => Task.FromResult(new HCPagedJobLogItems());

        public Task<HCJobStatus> GetJobStatusAsync(string id)
            => _jobStatuses.TryGetValue(id, out var status)
                ? Task.FromResult(status)
                : Task.FromResult<HCJobStatus>(null);

        public Task<List<HCJobStatus>> GetJobStatusesAsync()
            => Task.FromResult(_jobStatuses.Values.ToList());

        public async Task<HCJobStartResult> StartJobAsync(string jobId, HCJobStartCustomParameters parameters)
        {
            var context = new HCJobsContext();
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
                return new HCJobStartResult { Message = "Job is already running." };
            }

            var status = _jobStatuses[jobId];
            status.StartedAt = DateTimeOffset.Now;
            status.IsRunning = true;
            status.Summary = "Job started";
             
            await Task.Delay(TimeSpan.FromSeconds(_jobStatuses.Values.ToList().IndexOf(status) + 1));
            status.EndedAt = DateTimeOffset.Now;
            status.IsRunning = false;
            status.Summary = $"Finished - 'SomeString' parameter was '{parameters.GetParametersAs<DummyJobParameters>()?.SomeString}'";

            var items = Enumerable.Range(0, 8000)
                .Select((x, i) => new DummyItem
                {
                    EndedAt = DateTimeOffset.Now.AddMinutes(-i),
                    IsEnabled = i % 2 == 0,
                    IsRunning = i % 5 == 0,
                    JobId = $"{typeof(DummyJob).FullName}_{typeof(DummyJob).Assembly.ShortName()}",
                    SourceId = Guid.NewGuid().ToString(),
                    Summary = $"Summary here {Guid.NewGuid()}.",
                    StartedAt = DateTimeOffset.Now.AddDays(-1).AddHours(-5).AddMinutes(i),
                    LastRunWasSuccessful = i % 3 == 0,
                    NextExecutionScheduledAt = DateTimeOffset.Now.AddMinutes(5 + (i))
                })
                .ToArray();
            var html = new HtmlPresetBuilder()
                        //.AddItem(new HtmlPresetList().AddItem("Item A").AddItem("Item B").AddItem("Item C"))
                        //.AddItem(new HtmlPresetKeyValueList().AddItem("KeyA", "Value A").AddItem("KeyB", "Value B"))
                        //.AddItem(new HtmlPresetProgressbar("100", "75"))
                        //.AddItem(new HtmlPresetLink("https://localhost:7241/", "Some link"))
                        .AddItem(new HtmlPresetDataTable().AddItems(items))
                        .ToHtml();
            var data = JsonConvert.SerializeObject(status);

            var multiplier = jobId.EndsWith("_8") ? 500 : 1;
            for (int i = 0; i < multiplier; i++) {
                var stat = HCJobHistoryStatus.Success;
                if (i > 0) stat = (HCJobHistoryStatus)(i % 3);
                    
                var isHtml = false;
                if (jobId == "8")
                {
                    isHtml = true;
                    data = html;
                }

                await HCJobsUtils.StoreHistoryAsync<DummyJobsSource>(jobId, stat, $"Summary: {status.Summary}", data, isHtml, context: context);
            }

            return new HCJobStartResult { Success = true, Message = "Job was started." };
        }

        public class DummyJob {}

        public async Task<HCJobStopResult> StopJobAsync(string jobId)
        {
            var context = new HCJobsContext();
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
                return new HCJobStopResult { Message = "Job is not running." };
            }

            var status = _jobStatuses[jobId];
            status.EndedAt = DateTimeOffset.Now;
            status.IsRunning = false;
            status.Summary = "Job stopped";

            var stat = HCJobHistoryStatus.Success;
            var data = JsonConvert.SerializeObject(status);
            var isHtml = false;

            var multiplier = jobId.EndsWith("_8") ? 500 : 1;
            for (int i = 0; i < multiplier; i++)
            {
                if (i > 0) stat = (HCJobHistoryStatus)(i % 3);
                await HCJobsUtils.StoreHistoryAsync<DummyJobsSource>(jobId, stat, $"Summary: {status.Summary}", data, isHtml, context: context);
            }

            return new HCJobStopResult { Success = true, Message = "Job was stopped." };
        }

        public class DummyItem
        {
            public string SourceId { get; set; }
            public string JobId { get; set; }
            public string Summary { get; set; }
            public bool IsRunning { get; set; }
            public bool IsEnabled { get; set; }
            public DateTimeOffset? NextExecutionScheduledAt { get; set; }
            public DateTimeOffset? StartedAt { get; set; }
            public DateTimeOffset? EndedAt { get; set; }
            public bool? LastRunWasSuccessful { get; set; }
        }

        public class DummyJobParameters
        {
            public string SomeString { get; set; }
            public bool? SomeBool { get; set; }
            public DateTimeOffset? WhenAt { get; set; }
            [HCCustomProperty(UIHints = Core.Models.HCUIHint.CodeArea)]
            public string SomeCode{ get; set; }
        }
    }
}
