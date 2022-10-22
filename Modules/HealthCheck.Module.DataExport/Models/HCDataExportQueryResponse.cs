using HealthCheck.Core.Models;
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

        /// <summary>
        /// Optional note.
        /// </summary>
        public string Note { get; set; }

        /// <summary>
        /// Optionally force result headers.
        /// </summary>
        public List<HCTypeNamePair> AdditionalMembers { get; set; }
    }
}
