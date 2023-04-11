using QoDL.Toolkit.Module.DataExport.Abstractions;
using QoDL.Toolkit.Module.DataExport.Exporters;
using System.Collections.Generic;

namespace QoDL.Toolkit.Module.DataExport
{
    /// <summary>
    /// Options for <see cref="TKDataExportModule"/>.
    /// </summary>
    public class TKDataExportModuleOptions
    {
        /// <summary>
        /// Extra safety measure. Defaults to 1000.
        /// </summary>
        public int MaxPageSize { get; set; } = 1000;

        /// <summary>
        /// Service that handles the data.
        /// </summary>
        public ITKDataExportService Service { get; set; }

        /// <summary>
        /// Service that handles preset storage.
        /// </summary>
        public ITKDataExportPresetStorage PresetStorage { get; set; }

        /// <summary>
        /// Available exporters.
        /// <para>Defaults to all built in exporter types.</para>
        /// </summary>
        public List<ITKDataExportExporter> Exporters { get; set; } = new List<ITKDataExportExporter>
        {
            new TKDataExportExporterSCSV(),
            new TKDataExportExporterCSV(),
            new TKDataExportExporterTSV(),
            new TKDataExportExporterXml(),
            new TKDataExportExporterJsonArray(),
            new TKDataExportExporterJsonHash()
        };

        /// <summary>
        /// Adds the given exporter, by default at the top of the list.
        /// </summary>
        public TKDataExportModuleOptions AddExporter(ITKDataExportExporter exporter, bool insertFirst = true)
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
