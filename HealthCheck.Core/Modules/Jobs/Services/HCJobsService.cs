using HealthCheck.Core.Modules.Jobs.Abstractions;
using HealthCheck.Core.Modules.Jobs.Models;
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
        public async Task<List<HCJobHistoryEntry>> GetPagedHistoryAsync(string jobId, int pageIndex, int pageSize)
            => await _historyStorage.GetPagedHistoryAsync(jobId, pageIndex, pageSize);

        /// <inheritdoc />
        public async Task<HCJobHistoryDetailEntry> GetHistoryDetailAsync(Guid id)
            => await _historyDetailsStorage.GetDetailAsync(id);

        private string CreateSourceId(IHCJobsSource source)
            => source.GetType().FullName;
    }
}
