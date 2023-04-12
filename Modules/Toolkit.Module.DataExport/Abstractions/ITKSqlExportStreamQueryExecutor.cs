using QoDL.Toolkit.Module.DataExport.Abstractions.Streams;
using QoDL.Toolkit.Module.DataExport.Models;
using System.Threading.Tasks;

namespace QoDL.Toolkit.Module.DataExport.Abstractions;

/// <summary>
/// Performs SQL queries. Used by <see cref="TKSqlExportStreamBase{TParameters}"/>.
/// </summary>
public interface ITKSqlExportStreamQueryExecutor
{
    /// <summary>
    /// Execute a given query.
    /// </summary>
    Task<TKSqlExportStreamQueryExecutorResultModel> ExecuteQueryAsync(TKSqlExportStreamQueryExecutorQueryModel model);
}
