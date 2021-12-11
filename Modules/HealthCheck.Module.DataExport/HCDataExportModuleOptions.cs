using HealthCheck.Module.DataExport.Abstractions;
using System.Collections.Generic;
using static HealthCheck.Module.DataExport.HCDataExportModule;

namespace HealthCheck.Module.DataExport
{
    /// <summary>
    /// Options for <see cref="HCDataExportModule"/>.
    /// </summary>
    public class HCDataExportModuleOptions
    {
        /// <summary>
        /// Extra safety measure. Defaults to 1000.
        /// </summary>
        public int MaxPageSize { get; set; } = 1000;

        /// <summary>
        /// Service that handles the data.
        /// </summary>
        public IHCDataExportService Service { get; set; }

        /// <summary>
        /// Service that handles preset storage.
        /// </summary>
        public IHCDataExportPresetStorage PresetStorage { get; set; }

        /// <summary>
        /// Available exporters.
        /// <para>Defaults to <see cref="HCDataExportExporterCSV"/> only.</para>
        /// </summary>
        public IEnumerable<HCDataExportExporter> Exporters { get; set; } = new[]
        {
            new HCDataExportExporterCSV()
        };
    }
}
