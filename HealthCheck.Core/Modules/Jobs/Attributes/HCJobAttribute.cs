using HealthCheck.Core.Modules.Jobs.Services;
using System;

namespace HealthCheck.Core.Modules.Jobs.Attributes
{
    /// <summary>
    /// Used to provide some extra details to HC about this job.
    /// </summary>
    public class HCJobAttribute : Attribute
    {
        /// <summary>
        /// If set, inputs will be shown for this job and passed to <see cref="HCJobsService.StartJobAsync"/>.
        /// </summary>
        public Type CustomParametersType { get; set; }
    }
}
