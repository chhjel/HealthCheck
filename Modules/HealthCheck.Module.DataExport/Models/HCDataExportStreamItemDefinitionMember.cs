using HealthCheck.Core.Util;
using System;

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
        /// Type of the member.
        /// </summary>
        public Type Type { get; set; }

        /// <summary>
        /// Get the value of this member.
        /// </summary>
        public object GetValue(object rootInstance) => ReflectionUtils.GetValue(rootInstance, Name);
    }
}
