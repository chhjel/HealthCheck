using System.Collections.Generic;

namespace HealthCheck.Module.DataExport.Models
{
    /// <summary>
    /// Definition of an item model.
    /// </summary>
    public class HCDataExportStreamItemDefinition
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
        public List<HCDataExportStreamItemDefinitionMember> Members { get; set; } = new();
    }
}
