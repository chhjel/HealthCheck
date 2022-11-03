using HealthCheck.Core.Modules.Jobs.Models;
using System;
using System.Threading.Tasks;

namespace HealthCheck.Core.Modules.Jobs.Abstractions
{
    /// <summary>
    /// 
    /// </summary>
    public interface IHCJobsHistoryDetailsStorage
    {
        /// <summary></summary>
        Task InsertDetailAsync(HCJobHistoryDetailEntry detail);

        /// <summary></summary>
        Task<HCJobHistoryDetailEntry> GetDetailAsync(Guid id);

        /// <summary></summary>
        Task DeleteDetailAsync(Guid id);

        /// <summary></summary>
        Task DeleteAllDetailsAsync();
    }
}
