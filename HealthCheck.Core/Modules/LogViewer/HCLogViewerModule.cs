using HealthCheck.Core.Abstractions.Modules;
using HealthCheck.Core.Entities;
using HealthCheck.Core.Modules.LogViewer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HealthCheck.Core.Modules.LogViewer
{
    /// <summary>
    /// Module for viewing logfiles.
    /// </summary>
    public class HCLogViewerModule : HealthCheckModuleBase<HCLogViewerModule.AccessOption>
    {
        private HCLogViewerModuleOptions Options { get; }
        private static List<LogSearchInProgress> SearchesInProgress { get; set; } = new List<LogSearchInProgress>();

        /// <summary>
        /// Module for viewing logfiles.
        /// </summary>
        public HCLogViewerModule(HCLogViewerModuleOptions options)
        {
            Options = options;
        }

        /// <summary>
        /// Get frontend options for this module.
        /// </summary>
        public override object GetFrontendOptionsObject(AccessOption access) => new
        {
            CurrentlyRunningLogSearchCount = GetCurrentlyRunningLogSearchCount()
        };

        /// <summary>
        /// Get config for this module.
        /// </summary>
        public override IHealthCheckModuleConfig GetModuleConfig(AccessOption access) => new HCLogViewerModuleConfig();
        
        /// <summary>
        /// Different access options for this module.
        /// </summary>
        [Flags]
        public enum AccessOption
        {
            /// <summary>Does nothing.</summary>
            Nothing = 0,
        }

        #region Invokable methods
        /// <summary>
        /// Get log entry search results.
        /// </summary>
        [HealthCheckModuleMethod]
        public async Task<LogSearchResult> SearchLogs(HealthCheckModuleContext context, LogSearchFilter filter)
        {
            var result = await SearchLogsInternal(filter);

            if (!result.WasCancelled)
            {
                context.AddAuditEvent(action: "Searched logs", subject: filter?.Query)
                    .AddDetail("Skip", filter?.Skip.ToString() ?? "null")
                    .AddDetail("Take", filter?.Take.ToString() ?? "null")
                    .AddDetail("Range", $"{filter?.FromDate?.ToString() ?? "min"} -> {filter?.ToDate?.ToString() ?? "max"}")
                    .AddDetail("Result count", result?.Items?.Count.ToString() ?? "null")
                    .AddDetail("Duration", $"{result?.DurationInMilliseconds}ms");
            }

            return result;
        }

        /// <summary>
        /// Cancels the given log search.
        /// </summary>
        [HealthCheckModuleMethod]
        public bool CancelLogSearch(HealthCheckModuleContext context, string searchId)
        {
            var cancelled = CancelLogSearchInternal(searchId);
            if (cancelled)
            {
                context.AddAuditEvent("Cancelled log search");
            }
            return cancelled;
        }

        /// <summary>
        /// Cancels all log searches.
        /// </summary>
        [HealthCheckModuleMethod]
        public int CancelAllLogSearches(HealthCheckModuleContext context)
        {
            var count = AbortLogSearches();
            if (count > 0)
            {
                context.AddAuditEvent("Cancelled all log searches")
                    .AddDetail("Count", count.ToString());
            }
            return count;
        }
        #endregion

        #region Private helpers
        private class LogSearchInProgress
        {
            public string Id { get; set; }
            public DateTime StartedAt { get; set; }
            public CancellationTokenSource CancellationTokenSource { get; set; }
        }

        private async Task<LogSearchResult> SearchLogsInternal(LogSearchFilter filter)
        {
            CancellationTokenSource cts = new CancellationTokenSource();

            var search = new LogSearchInProgress
            {
                Id = filter.SearchId ?? Guid.NewGuid().ToString(),
                CancellationTokenSource = cts,
                StartedAt = DateTime.Now
            };
            lock (SearchesInProgress)
            {
                SearchesInProgress.Add(search);
            }

            var result = await Options.LogSearcherService.PerformSearchAsync(filter, cts.Token);

            lock (SearchesInProgress)
            {
                SearchesInProgress.Remove(search);

                // Cleanup any old searches
                lock (SearchesInProgress)
                {
                    AbortLogSearches(threshold: DateTime.Now.AddMinutes(-30));
                }
            }
            return result;
        }

        private bool CancelLogSearchInternal(string searchId)
        {
            lock (SearchesInProgress)
            {
                var search = SearchesInProgress.FirstOrDefault(x => x.Id == searchId);
                if (search == null)
                    return false;

                try
                {
                    search.CancellationTokenSource.Cancel();
                }
                catch (Exception) { }

                SearchesInProgress.Remove(search);
            }

            return true;
        }

        private int GetCurrentlyRunningLogSearchCount()
        {
            lock (SearchesInProgress)
            {
                return SearchesInProgress.Count;
            }
        }

        private int AbortLogSearches(DateTime? threshold = null)
        {
            lock (SearchesInProgress)
            {
                var searchesToCleanup = SearchesInProgress
                    .Where(x => threshold == null || x.StartedAt < threshold).ToList();

                foreach (var search in searchesToCleanup)
                {
                    CancelLogSearchInternal(search.Id);
                }

                return searchesToCleanup.Count;
            }
        }
        #endregion
    }
}
