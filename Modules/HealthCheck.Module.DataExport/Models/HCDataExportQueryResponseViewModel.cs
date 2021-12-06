using System.Collections.Generic;
using System.Linq;

namespace HealthCheck.Module.DataExport.Models
{
    /// <summary>
    /// Viewmodel for <see cref="HCDataExportQueryResponse"/>
    /// </summary>
    public class HCDataExportQueryResponseViewModel
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
