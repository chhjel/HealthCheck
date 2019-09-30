using HealthCheck.Core.Entities;
using HealthCheck.Core.Modules.LogViewer.Models;
using System.Threading.Tasks;

namespace HealthCheck.Core.Abstractions
{
    /// <summary>
    /// Searches logs for data.
    /// </summary>
    public interface ILogSearcherService
    {
        /// <summary>
        /// Search logs using the given filter.
        /// </summary>
        Task<LogSearchResult> PerformSearchAsync(LogSearchFilter filter);
    }
}
