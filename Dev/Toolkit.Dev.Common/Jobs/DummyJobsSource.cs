using Newtonsoft.Json;
using QoDL.Toolkit.Core.Attributes;
using QoDL.Toolkit.Core.Extensions;
using QoDL.Toolkit.Core.Modules.Jobs;
using QoDL.Toolkit.Core.Modules.Jobs.Abstractions;
using QoDL.Toolkit.Core.Modules.Jobs.Models;
using QoDL.Toolkit.Core.Modules.Tests.Utils.HtmlPresets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QoDL.Toolkit.Dev.Common.Jobs;

public class DummyJobsSource : ITKJobsSource
{
    public string Name { get; } = "Dummy Jobs";
    public string Description { get; } = "Dev job source.";
    private static Dictionary<string, TKJobStatus> _jobStatuses = new();

    public Task<List<TKJobDefinition>> GetJobDefinitionsAsync()
    {
        var jobs = Enumerable.Range(1, 8)
            .Select(x => new TKJobDefinition
            {
                Id = $"{typeof(DummyJob).FullName}_{typeof(DummyJob).Assembly.ShortName()}_{x}",
                Name = $"Some prefix: Job #{x} that does thing etc long text and maybe some more even",
                Description = string.Concat(Enumerable.Repeat($"Some description here for job #{x} ", x)),
                AllowedAccessRoles = null,
                GroupName = (x % 5 == 0) ? null : $"Group #{x % 2}",
                SupportsStart = x == 0 || x == 1 || x > 3,
                SupportsStop = x == 0 || x == 2 || x > 3,
                CustomParametersType = x % 2 == 0 ? typeof(DummyJobParameters) : null,
                CustomParameters = x % 2 == 0 ? TKCustomPropertyAttribute.CreateInputConfigs(typeof(DummyJobParameters)) : null
            })
            .ToList();
        return Task.FromResult(jobs);
    }

    public Task<TKPagedJobLogItems> GetJobLogItemsPaged(string jobId, int pageIndex, int pageSize)
        => Task.FromResult(new TKPagedJobLogItems());

    public Task<TKJobStatus> GetJobStatusAsync(string id)
        => _jobStatuses.TryGetValue(id, out var status)
            ? Task.FromResult(status)
            : Task.FromResult<TKJobStatus>(null);

    public Task<List<TKJobStatus>> GetJobStatusesAsync()
        => Task.FromResult(_jobStatuses.Values.ToList());

    public async Task<TKJobStartResult> StartJobAsync(string jobId, TKJobStartCustomParameters parameters)
    {
        var context = new TKJobsContext();
        if (!_jobStatuses.ContainsKey(jobId))
        {
            _jobStatuses[jobId] = new TKJobStatus
            {
                JobId = jobId,
                IsEnabled = true,
                NextExecutionScheduledAt = DateTime.Now.AddMinutes(12)
            };
        }

        if (_jobStatuses[jobId].IsRunning)
        {
            return new TKJobStartResult { Message = "Job is already running." };
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
        for (int i = 0; i < multiplier; i++)
        {
            var stat = TKJobHistoryStatus.Success;
            if (i > 0) stat = (TKJobHistoryStatus)(i % 3);

            var isHtml = false;
            if (jobId == "8")
            {
                isHtml = true;
                data = html;
            }

            await TKJobsUtils.StoreHistoryAsync<DummyJobsSource>(jobId, stat, $"Summary: {status.Summary}", data, isHtml, context: context);
        }

        return new TKJobStartResult { Success = true, Message = "Job was started." };
    }

    public class DummyJob { }

    public async Task<TKJobStopResult> StopJobAsync(string jobId)
    {
        var context = new TKJobsContext();
        if (!_jobStatuses.ContainsKey(jobId))
        {
            _jobStatuses[jobId] = new TKJobStatus
            {
                JobId = jobId,
                IsEnabled = true,
                NextExecutionScheduledAt = DateTime.Now.AddMinutes(12)
            };
        }

        if (!_jobStatuses[jobId].IsRunning)
        {
            return new TKJobStopResult { Message = "Job is not running." };
        }

        var status = _jobStatuses[jobId];
        status.EndedAt = DateTimeOffset.Now;
        status.IsRunning = false;
        status.Summary = "Job stopped";

        var stat = TKJobHistoryStatus.Success;
        var data = JsonConvert.SerializeObject(status);
        var isHtml = false;

        var multiplier = jobId.EndsWith("_8") ? 500 : 1;
        for (int i = 0; i < multiplier; i++)
        {
            if (i > 0) stat = (TKJobHistoryStatus)(i % 3);
            await TKJobsUtils.StoreHistoryAsync<DummyJobsSource>(jobId, stat, $"Summary: {status.Summary}", data, isHtml, context: context);
        }

        return new TKJobStopResult { Success = true, Message = "Job was stopped." };
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
        [TKCustomProperty(UIHints = Core.Models.TKUIHint.CodeArea)]
        public string SomeCode { get; set; }
    }
}
