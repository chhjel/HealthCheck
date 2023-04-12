using QoDL.Toolkit.Module.DataExport.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace QoDL.Toolkit.Module.DataExport.Abstractions;

/// <summary>
/// Handles preset storage.
/// </summary>
public interface ITKDataExportPresetStorage
{
    /// <summary>
    /// Get all presets for a given stream.
    /// </summary>
    Task<IEnumerable<TKDataExportStreamQueryPreset>> GetStreamQueryPresetsAsync(string streamId);

    /// <summary>
    /// Get a single preset.
    /// </summary>
    Task<TKDataExportStreamQueryPreset> GetStreamQueryPresetAsync(Guid id);

    /// <summary>
    /// Deletes a single preset.
    /// </summary>
    Task DeleteStreamQueryPresetAsync(Guid id);

    /// <summary>
    /// Save a single preset. Updates existing if found or creates a new if missing.
    /// </summary>
    Task<TKDataExportStreamQueryPreset> SaveStreamQueryPresetAsync(TKDataExportStreamQueryPreset preset);
}
