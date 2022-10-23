using HealthCheck.Module.DataExport.Abstractions.Streams;
using HealthCheck.Module.DataExport.Models;
using System.Threading.Tasks;

namespace HealthCheck.Module.DataExport.Abstractions
{
    /// <summary>
    /// Performs SQL queries. Used by <see cref="HCSqlExportStreamBase{TParameters}"/>.
    /// </summary>
    public interface IHCSqlExportStreamQueryExecutor
    {
        /// <summary>
        /// Execute a given query.
        /// </summary>
        Task<HCSqlExportStreamQueryExecutorResultModel> ExecuteQueryAsync(HCSqlExportStreamQueryExecutorQueryModel model);
    }
}
