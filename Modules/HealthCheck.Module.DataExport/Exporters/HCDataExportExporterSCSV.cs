using HealthCheck.Module.DataExport.Abstractions;

namespace HealthCheck.Module.DataExport.Exporters
{
    /// <summary>
    /// Outputs SCSV.
    /// </summary>
    public class HCDataExportExporterSCSV : HCDataExportExporterDelimitedSeparatedBase
    {
        /// <inheritdoc />
        public override string DisplayName { get; set; } = "Semicolon-separated values (CSV)";

        /// <inheritdoc />
        public override string Description { get; set; } = "Separates values using semicolon as delimiter.";

        /// <inheritdoc />
        public override string FileExtension { get; set; } = ".csv";

        /// <summary>
        /// Delimiter to separate values with.
        /// </summary>
        public override string Delimiter => ";";
    }
}
