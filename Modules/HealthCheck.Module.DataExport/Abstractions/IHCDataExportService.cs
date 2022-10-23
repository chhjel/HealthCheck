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
        HCDataExportStreamItemDefinition GetStreamItemDefinition(IHCDataExportStream stream, Type itemType);

        /// <summary>
        /// Get available streams.
        /// </summary>
        IEnumerable<IHCDataExportStream> GetStreams();

        /// <summary>
        /// Query for filtered items.
        /// </summary>
        Task<HCDataExportQueryResponse> QueryAsync(HCDataExportQueryRequest request);

        /// <summary>
        /// Get all supported formatter ids fro the given type.
        /// </summary>
        IEnumerable<string> GetValueFormatterIdsFor(IHCDataExportStream stream, Type type);
    }
}
