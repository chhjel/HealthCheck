using HealthCheck.Core.Modules.Jobs.Abstractions;
using HealthCheck.Core.Modules.Jobs.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HealthCheck.Dev.Common.Jobs
{
    public class DummyJobsHistoryDetailsStorage : IHCJobsHistoryDetailsStorage
    {
        private static List<HCJobHistoryDetailEntry> _items = new();

        public Task DeleteAllDetailsAsync()
        {
            lock (_items)
            {
                _items.Clear();
                return Task.CompletedTask;
            }
        }

        public Task DeleteDetailAsync(Guid id)
        {
            lock (_items)
            {
                _items.RemoveAll(x => x.Id == id);
                return Task.CompletedTask;
            }
        }

        public Task DeleteAllDetailsForJobAsync(string sourceId, string jobId)
        {
            lock (_items)
            {
                _items.RemoveAll(x => x.SourceId == sourceId && x.JobId == jobId);
                return Task.CompletedTask;
            }
        }

        public Task<HCJobHistoryDetailEntry> GetDetailAsync(Guid id)
        {
            lock (_items)
            {
                return Task.FromResult(_items.FirstOrDefault(x => x.Id == id));
            }
        }

        public Task<HCJobHistoryDetailEntry> InsertDetailAsync(HCJobHistoryDetailEntry detail)
        {
            lock (_items)
            {
                detail.Id = Guid.NewGuid();
                _items.Add(detail);
                return Task.FromResult(detail);
            }
        }
    }
}
