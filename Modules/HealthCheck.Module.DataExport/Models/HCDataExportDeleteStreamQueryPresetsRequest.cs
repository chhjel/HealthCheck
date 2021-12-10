using System;

namespace HealthCheck.Module.DataExport.Models
{
    /// <summary>
    /// Delete preset request.
    /// </summary>
    public class HCDataExportDeleteStreamQueryPresetsRequest
    {
        /// <summary>
        /// Id of the stream this preset belongs to.
        /// </summary>
        public string StreamId { get; set; }

        /// <summary>
        /// Preset id.
        /// </summary>
        public Guid Id { get; set; }
    }
}
