using HealthCheck.Core.Modules.Jobs.Abstractions;
using HealthCheck.Core.Modules.Jobs.Models;
using HealthCheck.Core.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HealthCheck.Core.Modules.Jobs.Services
{
    /// <summary>
    /// 
    /// </summary>
    public class HCJobsService : IHCJobsService
    {
        private readonly IEnumerable<IHCJobsSource> _jobSources;
        private readonly IHCJobsHistoryStorage _historyStorage;
        private readonly IHCJobsHistoryDetailsStorage _historyDetailsStorage;

        /// <summary></summary>
        public HCJobsService(IEnumerable<IHCJobsSource> jobSources, IHCJobsHistoryStorage historyStorage, IHCJobsHistoryDetailsStorage historyDetailsStorage)
        {
            _jobSources = jobSources;
            _historyStorage = historyStorage;
            _historyDetailsStorage = historyDetailsStorage;
        }

        /// <inheritdoc />
        public async Task<List<HCJobDefinitionWithSource>> GetJobDefinitions()
        {
            var defs = new List<HCJobDefinitionWithSource>();
            foreach (var source in _jobSources)
            {
                var items = (await source.GetJobDefinitionsAsync())
                    .Where(x => x != null)
                    .Select(x => new HCJobDefinitionWithSource
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
        public async Task<List<HCJobHistoryEntry>> GetLatestHistoryPerJobIdAsync()
            => await _historyStorage.GetLatestHistoryPerJobIdAsync();

        /// <inheritdoc />
        public async Task<HCPagedJobHistoryEntry> GetPagedHistoryAsync(string sourceId, string jobId, int pageIndex, int pageSize)
            => await _historyStorage.GetPagedHistoryAsync(sourceId, jobId, pageIndex, pageSize);
        
        /// <inheritdoc />
        public async Task<HCPagedJobLogItems> GetPagedJobLogItemsAsync(string sourceId, string jobId, int pageIndex, int pageSize)
        {
            var source = _jobSources.FirstOrDefault(x => CreateSourceId(x) == sourceId);
            HCPagedJobLogItems sourceResult = new();
            if (source != null)
            {
                sourceResult = await source.GetJobLogItemsPaged(jobId, pageIndex, pageSize);
            }
            return sourceResult;
        }

        /// <inheritdoc />
        public async Task<HCJobHistoryDetailEntry> GetHistoryDetailAsync(Guid id)
            => await _historyDetailsStorage.GetDetailAsync(id);

        /// <inheritdoc />
        public async Task<HCJobStatus> GetJobStatusAsync(string sourceId, string jobId)
        {
            var source = _jobSources.FirstOrDefault(x => CreateSourceId(x) == sourceId);
            if (source == null) return null;
            var statuse = await source.GetJobStatusAsync(jobId);
            statuse.SourceId = sourceId;
            return statuse;
        }

        /// <inheritdoc />
        public async Task<List<HCJobStatus>> GetJobStatusesAsync()
        {
            var statuses = new List<HCJobStatus>();
            foreach (var source in _jobSources)
            {
                var items = (await source.GetJobStatusesAsync());
                items.ForEach(x => x.SourceId = CreateSourceId(source));
                statuses.AddRange(items);
            }
            return statuses;
        }

        /// <inheritdoc />
        public async Task<HCJobStopResult> StopJobAsync(string sourceId, string jobId)
        {
            var source = _jobSources.FirstOrDefault(x => CreateSourceId(x) == sourceId);
            if (source == null) return new HCJobStopResult { Message = "Job not found." };
            return await source.StopJobAsync(jobId);
        }

        /// <inheritdoc />
        public async Task<HCJobStartResult> StartJobAsync(string sourceId, string jobId, Dictionary<string, string> parameters)
        {
            var source = _jobSources.FirstOrDefault(x => CreateSourceId(x) == sourceId);
            if (source == null) return new HCJobStartResult { Message = "Job not found." };

            var jobs = await source.GetJobDefinitionsAsync();
            var job = jobs.FirstOrDefault(x => x.Id == jobId);
            object parametersObject = job?.CustomParametersType == null ? null : HCValueConversionUtils.ConvertInputModel(job.CustomParametersType, parameters);
            var parametersModel = new HCJobStartCustomParameters
            {
                CustomParametersRaw = parameters,
                CustomParametersInstance = parametersObject,
                CustomParametersType = job?.CustomParametersType
            };

            return await source.StartJobAsync(jobId, parametersModel);
        }

        /// <inheritdoc />
        public async Task<HCJobSimpleResult> DeleteHistoryItemAsync(Guid id, Func<HCJobHistoryEntry, bool> condition)
        {
            var item = await _historyStorage.GetHistory(id);
            if (item != null)
            {
                return HCJobSimpleResult.CreateSuccess("History is already deleted or never existed.");
            }
            if (condition?.Invoke(item) != true)
            {
                return HCJobSimpleResult.CreateError("You do not have access to delete this item.");
            }

            await _historyStorage.DeleteHistoryItemAsync(id);
            if (item.DetailId != null)
            {
                await _historyDetailsStorage.DeleteDetailAsync(item.DetailId.Value);
            }
            return HCJobSimpleResult.CreateSuccess("History item deleted.");
        }

        /// <inheritdoc />
        public async Task<HCJobSimpleResult> DeleteAllHistoryForJobAsync(string sourceId, string jobId)
        {
            await _historyStorage.DeleteAllHistoryForJobAsync(sourceId, jobId);
            await _historyDetailsStorage.DeleteAllDetailsForJobAsync(sourceId, jobId);
            return HCJobSimpleResult.CreateSuccess("All history for job deleted.");
        }

        /// <inheritdoc />
        public async Task<HCJobSimpleResult> DeleteAllHistoryAsync()
        {
            await _historyStorage.DeleteAllHistoryAsync();
            await _historyDetailsStorage.DeleteAllDetailsAsync();
            return HCJobSimpleResult.CreateSuccess("All history deleted.");
        }

        private string CreateSourceId(IHCJobsSource source) => source.GetType().FullName;
    }
}
