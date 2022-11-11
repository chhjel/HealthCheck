using System;

namespace HealthCheck.Core.Modules.Jobs
{
    /// <summary>
    /// Helper that can be instantated at the start of the job to include timings etc.
    /// </summary>
    public class HCJobsContext
    {
        /// <summary>
        /// Automatically set to <c>DateTimeOffset.Now</c> on creation.
        /// </summary>
        public DateTimeOffset StartedAt { get; private set; } = DateTimeOffset.Now;
    }
}
