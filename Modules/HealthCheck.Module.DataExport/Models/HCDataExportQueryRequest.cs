using HealthCheck.Module.DataExport.Abstractions;

namespace HealthCheck.Module.DataExport.Models
{
    /// <summary>
    /// Model sent to <see cref="IHCDataExportService.QueryAsync"/>
    /// </summary>
    public class HCDataExportQueryRequest
    {
        /// <summary>
        /// Type of the stream.
        /// </summary>
        public string StreamId { get; set; }

        /// <summary>
        /// Page index to start at.
        /// </summary>
        public int PageIndex { get; set; }

        /// <summary>
        /// Page size to return.
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// Linq query.
        /// </summary>
        public string Query { get; set; }
    }
}
