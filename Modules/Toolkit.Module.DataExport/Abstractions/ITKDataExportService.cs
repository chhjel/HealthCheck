using QoDL.Toolkit.Module.DataExport.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace QoDL.Toolkit.Module.DataExport.Abstractions;

/// <summary>
/// Handles export stream data.
/// </summary>
public interface ITKDataExportService
{
    /// <summary>
    /// Create item definition for the given stream item type.
    /// </summary>
    TKDataExportStreamItemDefinition GetStreamItemDefinition(ITKDataExportStream stream, Type itemType);

    /// <summary>
    /// Get available streams.
    /// </summary>
    IEnumerable<ITKDataExportStream> GetStreams();

    /// <summary>
    /// Query for filtered items.
    /// </summary>
    Task<TKDataExportQueryResponse> QueryAsync(TKDataExportQueryRequest request);

    /// <summary>
    /// Get all supported formatter ids fro the given type.
    /// </summary>
    IEnumerable<string> GetValueFormatterIdsFor(ITKDataExportStream stream, Type type);
}
