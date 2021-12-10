namespace HealthCheck.Module.DataExport.Models
{
    /// <summary>
    /// Save preset request.
    /// </summary>
    public class HCDataExportSaveStreamQueryPresetRequest
    {
        /// <summary>
        /// Id of the stream this preset belongs to.
        /// </summary>
        public string StreamId { get; set; }

        /// <summary>
        /// Preset data.
        /// </summary>
        public HCDataExportStreamQueryPresetViewModel Preset { get; set; }
    }
}
