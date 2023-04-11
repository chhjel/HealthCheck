using QoDL.Toolkit.Core.Models;
using QoDL.Toolkit.Module.DataExport.Abstractions;
using System.Collections.Generic;
using System.Linq;

namespace QoDL.Toolkit.Module.DataExport.Models
{
    /// <summary>
    /// Model returned from <see cref="ITKDataExportService.QueryAsync"/>
    /// </summary>
    public class TKDataExportQueryResponse
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
        public List<TKTypeNamePair> AdditionalMembers { get; set; }
    }
}
