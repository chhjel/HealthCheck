﻿using HealthCheck.Module.DataExport.Abstractions;

namespace HealthCheck.Module.DataExport.Exporters
{
    /// <summary>
    /// Outputs TSV.
    /// </summary>
    public class HCDataExportExporterTSV : HCDataExportExporterDelimitedSeparatedBase
    {
        /// <inheritdoc />
        public override string DisplayName { get; set; } = "Tab-separated values (TSV)";

        /// <inheritdoc />
        public override string FileExtension { get; set; } = ".tsv";

        /// <summary>
        /// Delimiter to separate values with.
        /// </summary>
        public override string Delimiter => "\t";
    }
}