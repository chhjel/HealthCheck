using HealthCheck.Module.DataExport.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HealthCheck.Module.DataExport.Abstractions
{
    /// <summary>
    /// Handles preset storage.
    /// </summary>
    public interface IHCDataExportPresetStorage
    {
        /// <summary>
        /// Get all presets for a given stream.
        /// </summary>
        Task<IEnumerable<HCDataExportStreamQueryPreset>> GetStreamQueryPresetsAsync(string streamId);

        /// <summary>
        /// Get a single preset.
        /// </summary>
        Task<HCDataExportStreamQueryPreset> GetStreamQueryPresetAsync(Guid id);

        /// <summary>
        /// Deletes a single preset.
        /// </summary>
        Task DeleteStreamQueryPresetAsync(Guid id);

        /// <summary>
        /// Save a single preset. Updates existing if found or creates a new if missing.
        /// </summary>
        Task<HCDataExportStreamQueryPreset> SaveStreamQueryPresetAsync(HCDataExportStreamQueryPreset preset);
    }
}
