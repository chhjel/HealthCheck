using System.Collections.Generic;

namespace HealthCheck.Module.DataExport.Models
{
    /// <summary>
    /// Viewmodel for <see cref="HCDataExportStreamItemDefinition"/>
    /// </summary>
    public class HCDataExportStreamItemDefinitionViewModel
    {
        /// <summary>
        /// Id of the related stream.
        /// </summary>
        public string StreamId { get; set; }

        /// <summary>
        /// Name of the stream item.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// All members of the item.
        /// </summary>
        public List<HCDataExportStreamItemDefinitionMemberViewModel> Members { get; set; } = new();
    }
}
