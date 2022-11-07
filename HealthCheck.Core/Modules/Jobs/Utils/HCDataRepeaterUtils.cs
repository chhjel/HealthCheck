using HealthCheck.Core.Config;
using HealthCheck.Core.Modules.Jobs.Abstractions;
using HealthCheck.Core.Modules.Jobs.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace HealthCheck.Core.Modules.Jobs
{
    /// <summary>
    /// Utilities related to the jobs module.
    /// <para>Any exceptions ignored by the methods can be logged by subscribing to <see cref="HCGlobalConfig.OnExceptionEvent"/>.</para>
    /// </summary>
    public static class HCJobsUtils
    {
        /// <summary>
        /// Stores the given history data to be displayed in the jobs module.
        /// <para>Ignores any exception.</para>
        /// </summary>
        public static void StoreHistory<TJobSource>(string jobId,
           HCJobHistoryStatus status, string summary, string data, bool dataIsHtml = false, int historyCountLimit = 100)
            where TJobSource: IHCJobsSource
            => Task.Run(() => StoreHistoryAsync<TJobSource>(jobId, status, summary, data, dataIsHtml, historyCountLimit));

        /// <summary>
        /// Stores the given history data to be displayed in the jobs module.
        /// <para>Ignores any exception.</para>
        /// </summary>
        public static async Task<HCJobHistoryEntry> StoreHistoryAsync<TJobSource>(string jobId,
            HCJobHistoryStatus status, string summary, string data, bool dataIsHtml = false, int historyCountLimit = 100)
            where TJobSource : IHCJobsSource
        {
            var sourceId = typeof(TJobSource).FullName;
            var history = new HCJobHistoryEntry
            {
                SourceId = sourceId,
                JobId = jobId,
                Timestamp = DateTimeOffset.Now,
                Summary = summary,
                Status = status
            };
            var detail = new HCJobHistoryDetailEntry
            {
                SourceId = sourceId,
                JobId = jobId,
                Data = data,
                DataIsHtml = dataIsHtml
            };
            return await StoreHistoryAsync(history, detail, historyCountLimit);
        }

        /// <summary>
        /// Stores the given history data to be displayed in the jobs module.
        /// <para>Ignores any exception.</para>
        /// </summary>
        public static void StoreHistory(HCJobHistoryEntry history, HCJobHistoryDetailEntry detail, int historyCountLimit = 100)
            => Task.Run(() => StoreHistoryAsync(history, detail, historyCountLimit));

        /// <summary>
        /// Stores the given history data to be displayed in the jobs module.
        /// <para>Ignores any exception.</para>
        /// </summary>
        public static async Task<HCJobHistoryEntry> StoreHistoryAsync(HCJobHistoryEntry history, HCJobHistoryDetailEntry detail, int historyCountLimit = 100)
        {
            try
            {
                if (HCGlobalConfig.GetDefaultInstanceResolver()?.Invoke(typeof(IHCJobsHistoryStorage)) is not IHCJobsHistoryStorage historyStorage) return null;
                if (HCGlobalConfig.GetDefaultInstanceResolver()?.Invoke(typeof(IHCJobsHistoryDetailsStorage)) is not IHCJobsHistoryDetailsStorage detailsStorage) return null;

                detail = await detailsStorage.InsertDetailAsync(detail);
                history.DetailId = detail.Id;
                history = await historyStorage.InsertHistoryAsync(history);

                var deletedHistories = await historyStorage.LimitMaxHistoryCountForJob(history.SourceId, history.JobId, historyCountLimit);
                foreach (var deletedHistory in deletedHistories.Where(x => x.DetailId != null))
                {
                    await detailsStorage.DeleteDetailAsync(deletedHistory.DetailId.Value);
                }

                return history;
            }
            catch (Exception ex)
            {
                HCGlobalConfig.OnExceptionEvent?.Invoke(typeof(HCJobsUtils), nameof(StoreHistoryAsync), ex);
                return null;
            }
        }
    }
}
