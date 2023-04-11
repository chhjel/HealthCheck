using QoDL.Toolkit.Core.Modules.Jobs.Models;
using System;
using System.Threading.Tasks;

namespace QoDL.Toolkit.Core.Modules.Jobs.Abstractions
{
    /// <summary>
    /// 
    /// </summary>
    public interface ITKJobsHistoryDetailsStorage
    {
        /// <summary></summary>
        Task<TKJobHistoryDetailEntry> InsertDetailAsync(TKJobHistoryDetailEntry detail);

        /// <summary></summary>
        Task<TKJobHistoryDetailEntry> GetDetailAsync(Guid id);

        /// <summary></summary>
        Task DeleteDetailAsync(Guid id);

        /// <summary></summary>
        Task DeleteAllDetailsAsync();

        /// <summary></summary>
        Task DeleteAllDetailsForJobAsync(string sourceId, string jobId);
    }
}
