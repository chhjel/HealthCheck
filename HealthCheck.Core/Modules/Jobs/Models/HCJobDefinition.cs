using HealthCheck.Core.Util.Models;
using System;
using System.Collections.Generic;

namespace HealthCheck.Core.Modules.Jobs.Models
{
    /// <summary>
    /// 
    /// </summary>
    public class HCJobDefinition
    {
        /// <summary></summary>
        public string Id { get; set; }

        /// <summary></summary>
        public string Name { get; set; }

        /// <summary></summary>
        public string Description { get; set; }

        /// <summary>
        /// Name of the group this job belongs to if any.
        /// </summary>
        public string GroupName { get; set; }

        /// <inheritdoc />
        public virtual List<string> Categories { get; } = new();

        /// <summary>
        /// Optional access roles that can access this job.
        /// <para>Must be a flags enum of the same type as the one used on the healthcheck controller.</para>
        /// </summary>
        public object AllowedAccessRoles { get; set; }

        /// <summary></summary>
        public bool SupportsStart { get; set; }

        /// <summary></summary>
        public bool SupportsStop { get; set; }

        /// <summary></summary>
        public Type CustomParametersType { get; set; }

        /// <summary></summary>
        public List<HCBackendInputConfig> CustomParameters { get; set; }
    }
}
