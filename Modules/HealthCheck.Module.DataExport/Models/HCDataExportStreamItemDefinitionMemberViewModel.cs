﻿namespace HealthCheck.Module.DataExport.Models
{
    /// <summary>
    /// Viewmodel for <see cref="HCDataExportStreamItemDefinitionMember"/>,
    /// </summary>
    public class HCDataExportStreamItemDefinitionMemberViewModel
    {
        /// <summary>
        /// Name of the member.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Type of the member.
        /// </summary>
        public string TypeName { get; set; }
    }
}