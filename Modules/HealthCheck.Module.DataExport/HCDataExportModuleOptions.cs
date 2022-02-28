using HealthCheck.Module.DataExport.Abstractions;
using HealthCheck.Module.DataExport.Exporters;
using System.Collections.Generic;

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
        /// <para>Defaults to all built in exporter types.</para>
        /// </summary>
        public List<IHCDataExportExporter> Exporters { get; set; } = new List<IHCDataExportExporter>
        {
            new HCDataExportExporterSCSV(),
            new HCDataExportExporterCSV(),
            new HCDataExportExporterTSV(),
            new HCDataExportExporterXml(),
            new HCDataExportExporterJsonArray(),
            new HCDataExportExporterJsonHash()
        };

        /// <summary>
        /// Adds the given exporter, by default at the top of the list.
        /// </summary>
        public HCDataExportModuleOptions AddExporter(IHCDataExportExporter exporter, bool insertFirst = true)
        {
            if (insertFirst)
            {
                Exporters.Insert(0, exporter);
            }
            else
            {
               Exporters.Add(exporter);
            }
            return this;
        }
    }
}
