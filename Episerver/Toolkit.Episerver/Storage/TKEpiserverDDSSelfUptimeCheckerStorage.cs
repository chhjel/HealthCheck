using EPiServer.Data.Dynamic;
using QoDL.Toolkit.Core.Abstractions;
using QoDL.Toolkit.Episerver.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace QoDL.Toolkit.Episerver.Storage;

/// <summary>
/// Stores data in epi DDS.
/// </summary>
public class TKEpiserverDDSSelfUptimeCheckerStorage : ITKSelfUptimeCheckerStorage
{
    /// <inheritdoc />
    public Task<DateTimeOffset?> GetLastCheckedAtAsync()
    {
        var store = typeof(TKSelfUptimeData).GetOrCreateStore();
        var data = store.Items<TKSelfUptimeData>().FirstOrDefault();
        return Task.FromResult(data?.LastCheckedAt);
    }

    /// <inheritdoc />
    public Task StoreLastCheckedAtAsync(DateTimeOffset time)
    {
        var store = typeof(TKSelfUptimeData).GetOrCreateStore();
        var data = store.Items<TKSelfUptimeData>().FirstOrDefault();

        if (data == null)
        {
            var newData = new TKSelfUptimeData { LastCheckedAt = time };
            store.Save(newData);
        }
        else
        {
            data.LastCheckedAt = time;
            store.Save(data);
        }

        return Task.CompletedTask;
    }
}
