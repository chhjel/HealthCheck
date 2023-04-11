using System.Collections.Generic;

namespace QoDL.Toolkit.Module.DataExport.Models
{
    /// <summary>
    /// Viewmodel for <see cref="TKDataExportStreamItemDefinitionMember"/>,
    /// </summary>
    public class TKDataExportStreamItemDefinitionMemberViewModel
    {
        /// <summary>
        /// Name of the member.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Type of the member.
        /// </summary>
        public string TypeName { get; set; }

        /// <summary>
        /// Supported formatters.
        /// </summary>
        public IEnumerable<string> FormatterIds { get; set; }
    }
}
