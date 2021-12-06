using HealthCheck.Module.DataExport.Abstractions;
using System.Collections.Generic;
using System.Linq;

namespace HealthCheck.Module.DataExport.Models
{
    /// <summary>
    /// Model returned from <see cref="IHCDataExportService.QueryAsync"/>
    /// </summary>
    public class HCDataExportQueryResponse
    {
        /// <summary>
        /// Total count.
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// Items on the current page.
        /// </summary>
        public IEnumerable<object> Items { get; set; } = Enumerable.Empty<object>();
    }
}
