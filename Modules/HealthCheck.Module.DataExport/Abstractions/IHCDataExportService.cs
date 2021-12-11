using HealthCheck.Module.DataExport.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HealthCheck.Module.DataExport.Abstractions
{
    /// <summary>
    /// Handles export stream data.
    /// </summary>
    public interface IHCDataExportService
    {
        /// <summary>
        /// Create item definition for the given stream item type.
        /// </summary>
        HCDataExportStreamItemDefinition GetStreamItemDefinition(string streamId, Type itemType);

        /// <summary>
        /// Get available streams.
        /// </summary>
        IEnumerable<IHCDataExportStream> GetStreams();

        /// <summary>
        /// Query for filtered items.
        /// </summary>
        Task<HCDataExportQueryResponse> QueryAsync(HCDataExportQueryRequest request);
    }
}
