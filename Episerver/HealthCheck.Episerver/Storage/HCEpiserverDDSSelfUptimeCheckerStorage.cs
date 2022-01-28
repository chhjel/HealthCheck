using EPiServer.Data.Dynamic;
using HealthCheck.Core.Abstractions;
using HealthCheck.Episerver.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace HealthCheck.Episerver.Storage
{
    /// <summary>
    /// Stores data in epi DDS.
    /// </summary>
    public class HCEpiserverDDSSelfUptimeCheckerStorage : IHCSelfUptimeCheckerStorage
    {
        /// <inheritdoc />
        public Task<DateTimeOffset?> GetLastCheckedAtAsync()
        {
            var store = typeof(HCSelfUptimeData).GetOrCreateStore();
            var data = store.Items<HCSelfUptimeData>().FirstOrDefault();
            return Task.FromResult(data?.LastCheckedAt);
        }

        /// <inheritdoc />
        public Task StoreLastCheckedAtAsync(DateTimeOffset time)
        {
            var store = typeof(HCSelfUptimeData).GetOrCreateStore();
            var data = store.Items<HCSelfUptimeData>().FirstOrDefault();

            if (data == null)
            {
                var newData = new HCSelfUptimeData { LastCheckedAt = time };
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
}
