using System.Collections.Generic;

namespace QoDL.Toolkit.Module.DataExport.Models;

/// <summary></summary>
public class TKSqlExportStreamQueryExecutorResultModel
{
    /// <summary></summary>
    public int TotalCount { get; set; }
    /// <summary></summary>
    public int RecordsAffected { get; set; }
    /// <summary></summary>
    public List<TKSqlExportStreamQueryExecutorResultColumnModel> Columns { get; set; }
    /// <summary></summary>
    public List<List<object>> Rows { get; set; }
}
