using QoDL.Toolkit.Module.DataExport.Abstractions;

namespace QoDL.Toolkit.Module.DataExport.Exporters
{
    /// <summary>
    /// Outputs CSV.
    /// </summary>
    public class TKDataExportExporterCSV : TKDataExportExporterDelimitedSeparatedBase
    {
        /// <inheritdoc />
        public override string DisplayName { get; set; } = "Comma-separated values (CSV)";

        /// <inheritdoc />
        public override string Description { get; set; } = "Separates values using comma as delimiter.";

        /// <inheritdoc />
        public override string FileExtension { get; set; } = ".csv";

        /// <summary>
        /// Delimiter to separate values with.
        /// </summary>
        public override string Delimiter => ",";
    }
}
