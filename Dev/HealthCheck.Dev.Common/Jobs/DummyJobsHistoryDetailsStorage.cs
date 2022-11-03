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
            _items.Clear();
            return Task.CompletedTask;
        }

        public Task DeleteDetailAsync(Guid id)
        {
            _items.RemoveAll(x => x.Id == id);
            return Task.CompletedTask;
        }

        public Task<HCJobHistoryDetailEntry> GetDetailAsync(Guid id)
            => Task.FromResult(_items.FirstOrDefault(x => x.Id == id));

        public Task InsertDetailAsync(HCJobHistoryDetailEntry detail)
        {
            _items.Add(detail);
            return Task.CompletedTask;
        }
    }
}
