using HealthCheck.Core.Modules.Jobs.Abstractions;

namespace HealthCheck.Core.Modules.Jobs
{
    /// <summary>
    /// Options for <see cref="HCJobsModule"/>.
    /// </summary>
    public class HCJobsModuleOptions
    {
        /// <summary>
        /// 
        /// </summary>
        public IHCJobsService Service { get; set; }
    }
}