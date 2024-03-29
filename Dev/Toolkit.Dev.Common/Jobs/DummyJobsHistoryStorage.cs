using QoDL.Toolkit.Core.Modules.Jobs.Abstractions;
using QoDL.Toolkit.Core.Modules.Jobs.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QoDL.Toolkit.Dev.Common.Jobs;

public class DummyJobsHistoryStorage : ITKJobsHistoryStorage
{
    private static List<TKJobHistoryEntry> _items = new();

    public Task DeleteAllHistoryAsync()
    {
        lock (_items)
        {
            _items.Clear();
            return Task.CompletedTask;
        }
    }

    public Task DeleteAllHistoryForJobAsync(string sourceId, string jobId)
    {
        lock (_items)
        {
            _items.RemoveAll(x => x.SourceId == sourceId && x.JobId == jobId);
            return Task.CompletedTask;
        }
    }

    public Task DeleteHistoryItemAsync(Guid id)
    {
        lock (_items)
        {
            _items.RemoveAll(x => x.Id == id);
            return Task.CompletedTask;
        }
    }

    public Task<TKPagedJobHistoryEntry> GetPagedHistoryAsync(string sourceId, string jobId, int pageIndex, int pageSize)
    {
        lock (_items)
        {
            var potential = _items
                .OrderByDescending(h => h.EndedAt)
                .Where(x => x.SourceId == sourceId && x.JobId == jobId);
            var items = potential
                .Skip(pageIndex * pageSize)
                .Take(pageSize)
                .ToList();
            return Task.FromResult(new TKPagedJobHistoryEntry
            {
                Items = items,
                TotalCount = potential.Count()
            });
        }
    }

    public Task<List<TKJobHistoryEntry>> GetLatestHistoryPerJobIdAsync()
    {
        lock (_items)
        {
            var items = _items
                .GroupBy(x => x.JobId)
                .Select(x => x.OrderByDescending(h => h.EndedAt).First())
                .ToList();
            return Task.FromResult(items);
        }
    }

    public Task<TKJobHistoryEntry> InsertHistoryAsync(TKJobHistoryEntry history)
    {
        lock (_items)
        {
            history.Id = Guid.NewGuid();
            _items.Add(history);
            return Task.FromResult(history);
        }
    }

    public Task<TKJobHistoryEntry> GetHistory(Guid id)
    {
        lock (_items)
        {
            return Task.FromResult(_items.FirstOrDefault(x => x.Id == id));
        }
    }

    public Task<IEnumerable<TKJobHistoryEntry>> LimitMaxHistoryCountForJob(string sourceId, string jobId, int maxCount)
    {
        lock (_items)
        {
            var potental = _items.Where(x => x.SourceId == sourceId && x.JobId == jobId);
            var toDelete = potental.Reverse().Skip(maxCount).ToList();
            toDelete.ForEach(x => _items.Remove(x));
            return Task.FromResult<IEnumerable<TKJobHistoryEntry>>(toDelete);
        }
    }
}
