namespace QoDL.Toolkit.Module.DataExport.Models;

/// <summary></summary>
public class TKSqlExportStreamQueryExecutorQueryModel
{
    /// <summary></summary>
    public string ConnectionString { get; set; }
    /// <summary></summary>
    public string QuerySelectTotalCount { get; set; }
    /// <summary></summary>
    public string QuerySelectData { get; set; }
    /// <summary></summary>
    public string QueryPredicate { get; set; }
    /// <summary></summary>
    public int PageIndex { get; set; }
    /// <summary></summary>
    public int PageSize { get; set; }
}
