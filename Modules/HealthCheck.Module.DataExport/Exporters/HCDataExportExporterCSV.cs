using HealthCheck.Module.DataExport.Abstractions;

namespace HealthCheck.Module.DataExport.Exporters
{
    /// <summary>
    /// Outputs CSV.
    /// </summary>
    public class HCDataExportExporterCSV : HCDataExportExporterDelimitedSeparatedBase
    {
        /// <inheritdoc />
        public override string DisplayName { get; set; } = "Comma-separated values (CSV)";

        /// <inheritdoc />
        public override string FileExtension { get; set; } = ".csv";

        /// <summary>
        /// Delimiter to separate values with.
        /// </summary>
        public override string Delimiter => ";";
    }
}
