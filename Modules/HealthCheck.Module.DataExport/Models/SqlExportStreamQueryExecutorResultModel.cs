using System.Collections.Generic;

namespace HealthCheck.Module.DataExport.Models
{
    /// <summary></summary>
    public class HCSqlExportStreamQueryExecutorResultModel
    {
        /// <summary></summary>
        public int TotalCount { get; set; }
        /// <summary></summary>
        public int RecordsAffected { get; set; }
        /// <summary></summary>
        public List<HCSqlExportStreamQueryExecutorResultColumnModel> Columns { get; set; }
        /// <summary></summary>
        public List<List<object>> Rows { get; set; }
    }
}
