using QoDL.Toolkit.Core.Modules.LogViewer.Models;
using System.Threading;
using System.Threading.Tasks;

namespace QoDL.Toolkit.Core.Abstractions
{
    /// <summary>
    /// Searches logs for data.
    /// </summary>
    public interface ILogSearcherService
    {
        /// <summary>
        /// Search logs using the given filter.
        /// </summary>
        Task<LogSearchResult> PerformSearchAsync(LogSearchFilter filter, CancellationToken cancellationToken);
    }
}
