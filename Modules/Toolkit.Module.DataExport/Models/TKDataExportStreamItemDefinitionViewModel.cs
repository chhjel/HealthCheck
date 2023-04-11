using System.Collections.Generic;

namespace QoDL.Toolkit.Module.DataExport.Models
{
    /// <summary>
    /// Viewmodel for <see cref="TKDataExportStreamItemDefinition"/>
    /// </summary>
    public class TKDataExportStreamItemDefinitionViewModel
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
        public List<TKDataExportStreamItemDefinitionMemberViewModel> Members { get; set; } = new();
    }
}
