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
        /// True if nothing failed.
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Error if any.
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// Error details if any.
        /// </summary>
        public string ErrorDetails { get; set; }

        /// <summary>
        /// Total count.
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// Items on the current page.
        /// </summary>
        public IEnumerable<object> Items { get; set; } = Enumerable.Empty<object>();

        /// <summary>
        /// Creates a new error.
        /// </summary>
        public static HCDataExportQueryResponseViewModel CreateError(string message) => new()
        {
            ErrorMessage = message
        };
    }
}
