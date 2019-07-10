﻿using HealthCheck.Core.Abstractions;
using HealthCheck.Core.Entities;
using HealthCheck.Core.Util;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HealthCheck.WebUI.Services
{
    /// <summary>
    /// Stores events to the given file.
    /// </summary>
    public class FlatFileSiteEventStorageService : ISiteEventService
    {
        /// <summary>
        /// Max age of events before they can become deleted.
        /// <para>Delete check is once per min(MaxEventAge, 4 hours) whenever a new event is stored.</para>
        /// </summary>
        public TimeSpan? MaxEventAge { get; set; }

        private bool CleanupEnabled => MaxEventAge != null;
        private DateTime? LastCleanup { get; set; }
        private SimpleDataStore<SiteEvent> Store { get; set; }

        /// <summary>
        /// Create a new <see cref="FlatFileSiteEventStorageService"/> with the given json file path.
        /// </summary>
        public FlatFileSiteEventStorageService(string filepath)
        {
            Store = new SimpleDataStore<SiteEvent>(
                filepath,
                serializer: new Func<SiteEvent, string>((e) => JsonConvert.SerializeObject(e)),
                deserializer: new Func<string, SiteEvent>((row) => JsonConvert.DeserializeObject<SiteEvent>(row))
            );
        }

        /// <summary>
        /// Store the given event. There is a 2 second buffer delay before the item is written.
        /// </summary>
        public Task StoreEvent(SiteEvent siteEvent)
        {
            Store.InsertItem(siteEvent);
            CheckCleanup();
            return Task.CompletedTask;
        }

        /// <summary>
        /// Get stored events within the given threshold.
        /// </summary>
        public Task<List<SiteEvent>> GetEvents(DateTime from, DateTime to)
        {
            var items = Store.GetEnumerable()
                .Where(x => x.Timestamp >= from && x.Timestamp <= to)
                .ToList();

            return Task.FromResult(items);
        }

        private void CheckCleanup()
        {
            var minimumCleanupInterval = TimeSpan.FromHours(4);
            if (MaxEventAge != null && MaxEventAge < minimumCleanupInterval)
            {
                minimumCleanupInterval = MaxEventAge.Value;
            }

            // Cleanup disabled => abort
            if (!CleanupEnabled) return;
            // Less than min time since last cleanup => abort
            else if (LastCleanup != null && (DateTime.Now - LastCleanup) < minimumCleanupInterval) return;

            var threshold = DateTime.Now - MaxEventAge;
            Store.DeleteWhere(x => x.Timestamp <= threshold);

            LastCleanup = DateTime.Now;
        }
    }
}
