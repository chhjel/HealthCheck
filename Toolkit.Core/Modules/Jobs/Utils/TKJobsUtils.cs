using QoDL.Toolkit.Core.Config;
using QoDL.Toolkit.Core.Extensions;
using QoDL.Toolkit.Core.Modules.Jobs.Abstractions;
using QoDL.Toolkit.Core.Modules.Jobs.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace QoDL.Toolkit.Core.Modules.Jobs
{
    /// <summary>
    /// Utilities related to the jobs module.
    /// <para>Any exceptions ignored by the methods can be logged by subscribing to <see cref="TKGlobalConfig.OnExceptionEvent"/>.</para>
    /// </summary>
    public static class TKJobsUtils
    {
        /// <summary>
        /// Stores the given history data to be displayed in the jobs module.
        /// <para>Ignores any exception.</para>
        /// </summary>
        public static void StoreHistory<TJobSource, TJob>(TKJobHistoryStatus status, string summary, string data, bool dataIsHtml = false, int historyCountLimit = 100,
           TKJobsContext context = null)
            where TJobSource: ITKJobsSource
            => Task.Run(() => StoreHistoryAsync<TJobSource, TJob>(status, summary, data, dataIsHtml, historyCountLimit, context));

        /// <summary>
        /// Stores the given history data to be displayed in the jobs module.
        /// <para>Ignores any exception.</para>
        /// </summary>
        public static async Task<TKJobHistoryEntry> StoreHistoryAsync<TJobSource, TJob>(
            TKJobHistoryStatus status, string summary, string data, bool dataIsHtml = false, int historyCountLimit = 100,
           TKJobsContext context = null)
            where TJobSource : ITKJobsSource
        {
            var jobId = CreateJobId<TJob>();
            return await StoreHistoryAsync<TJobSource>(jobId, status, summary, data, dataIsHtml, historyCountLimit, context);
        }

        /// <summary>
        /// Stores the given history data to be displayed in the jobs module.
        /// <para>Ignores any exception.</para>
        /// </summary>
        public static void StoreHistory<TJobSource>(string jobId, TKJobHistoryStatus status, string summary, string data, bool dataIsHtml = false, int historyCountLimit = 100,
           TKJobsContext context = null)
            where TJobSource : ITKJobsSource
            => Task.Run(() => StoreHistoryAsync<TJobSource>(jobId, status, summary, data, dataIsHtml, historyCountLimit, context));

        /// <summary>
        /// Stores the given history data to be displayed in the jobs module.
        /// <para>Ignores any exception.</para>
        /// </summary>
        public static async Task<TKJobHistoryEntry> StoreHistoryAsync<TJobSource>(string jobId,
            TKJobHistoryStatus status, string summary, string data, bool dataIsHtml = false, int historyCountLimit = 100,
           TKJobsContext context = null)
            where TJobSource : ITKJobsSource
        {
            var sourceId = typeof(TJobSource).FullName;
            var history = new TKJobHistoryEntry
            {
                SourceId = sourceId,
                JobId = jobId,
                StartedAt = context?.StartedAt.UtcDateTime,
                EndedAt = DateTimeOffset.UtcNow,
                Summary = summary,
                Status = status
            };
            var detail = new TKJobHistoryDetailEntry
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
        public static void StoreHistory(TKJobHistoryEntry history, TKJobHistoryDetailEntry detail, int historyCountLimit = 100)
            => Task.Run(() => StoreHistoryAsync(history, detail, historyCountLimit));

        /// <summary>
        /// Stores the given history data to be displayed in the jobs module.
        /// <para>Ignores any exception.</para>
        /// </summary>
        public static async Task<TKJobHistoryEntry> StoreHistoryAsync(TKJobHistoryEntry history, TKJobHistoryDetailEntry detail, int historyCountLimit = 100)
        {
            try
            {
                if (TKGlobalConfig.GetDefaultInstanceResolver()?.Invoke(typeof(ITKJobsHistoryStorage)) is not ITKJobsHistoryStorage historyStorage) return null;
                if (TKGlobalConfig.GetDefaultInstanceResolver()?.Invoke(typeof(ITKJobsHistoryDetailsStorage)) is not ITKJobsHistoryDetailsStorage detailsStorage) return null;

                history.Id = Guid.NewGuid();
                detail.Id = Guid.NewGuid();

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
                TKGlobalConfig.OnExceptionEvent?.Invoke(typeof(TKJobsUtils), nameof(StoreHistoryAsync), ex);
                return null;
            }
        }

        internal static string CreateJobId<TJob>() => CreateJobId(typeof(TJob));
        internal static string CreateJobId(Type type) => $"{type.FullName}_{type.Assembly.ShortName()}";
    }
}
