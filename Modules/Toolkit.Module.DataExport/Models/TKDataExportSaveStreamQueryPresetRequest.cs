namespace QoDL.Toolkit.Module.DataExport.Models;

/// <summary>
/// Save preset request.
/// </summary>
public class TKDataExportSaveStreamQueryPresetRequest
{
    /// <summary>
    /// Id of the stream this preset belongs to.
    /// </summary>
    public string StreamId { get; set; }

    /// <summary>
    /// Preset data.
    /// </summary>
    public TKDataExportStreamQueryPresetViewModel Preset { get; set; }
}
