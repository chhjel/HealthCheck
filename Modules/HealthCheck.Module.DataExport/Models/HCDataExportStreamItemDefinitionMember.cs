using HealthCheck.Core.Util;
using System;
using System.Collections.Generic;

namespace HealthCheck.Module.DataExport.Models
{
    /// <summary>
    /// Definition of an item model member.
    /// </summary>
    public class HCDataExportStreamItemDefinitionMember
    {
        /// <summary>
        /// Name of the member.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Name of the member with any indices cleaned.
        /// </summary>
        public string NameWithCleanIndices { get; set; }

        /// <summary>
        /// Type of the member.
        /// </summary>
        public Type Type { get; set; }

        /// <summary>
        /// Supported formatters.
        /// </summary>
        public IEnumerable<string> FormatterIds { get; set; }

        /// <summary>
        /// Get the value of this member.
        /// </summary>
        public object GetValue(object rootInstance) => HCReflectionUtils.GetValue(rootInstance, Name);

        /// <summary>
        /// Returns a copy.
        /// </summary>
        public HCDataExportStreamItemDefinitionMember Clone() => new()
        {
            Name = Name,
            NameWithCleanIndices = NameWithCleanIndices,
            FormatterIds = FormatterIds,
            Type = Type
        };
    }
}
