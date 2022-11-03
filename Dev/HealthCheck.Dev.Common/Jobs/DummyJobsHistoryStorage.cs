using HealthCheck.Core.Modules.Jobs.Abstractions;
using HealthCheck.Core.Modules.Jobs.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HealthCheck.Dev.Common.Jobs
{
    public class DummyJobsHistoryStorage : IHCJobsHistoryStorage
    {
        private static List<HCJobHistoryEntry> _items = new();

        public Task DeleteAllHistoryAsync()
        {
            _items.Clear();
            return Task.CompletedTask;
        }

        public Task DeleteAllHistoryForJobAsync(string jobId)
        {
            _items.RemoveAll(x => x.JobId == jobId);
            return Task.CompletedTask;
        }

        public Task DeleteHistoryItemAsync(Guid id)
        {
            _items.RemoveAll(x => x.Id == id);
            return Task.CompletedTask;
        }

        public Task<List<HCJobHistoryEntry>> GetPagedHistoryAsync(string jobId, int pageIndex, int pageSize)
        {
            var items = _items
                .Where(x => x.JobId == jobId)
                .Skip(pageIndex * pageSize)
                .Take(pageSize)
                .ToList();
            return Task.FromResult(items);
        }

        public Task InsertHistoryAsync(HCJobHistoryEntry history)
        {
            _items.Add(history);
            return Task.CompletedTask;
        }
    }
}
