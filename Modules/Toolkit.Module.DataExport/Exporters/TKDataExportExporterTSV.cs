using QoDL.Toolkit.Module.DataExport.Abstractions;

namespace QoDL.Toolkit.Module.DataExport.Exporters;

/// <summary>
/// Outputs TSV.
/// </summary>
public class TKDataExportExporterTSV : TKDataExportExporterDelimitedSeparatedBase
{
    /// <inheritdoc />
    public override string DisplayName { get; set; } = "Tab-separated values (TSV)";

    /// <inheritdoc />
    public override string Description { get; set; } = "Separates values using tabs as delimiter.";

    /// <inheritdoc />
    public override string FileExtension { get; set; } = ".tsv";

    /// <summary>
    /// Delimiter to separate values with.
    /// </summary>
    public override string Delimiter => "\t";
}
