using QoDL.Toolkit.Core.Modules.Jobs.Abstractions;

namespace QoDL.Toolkit.Core.Modules.Jobs
{
    /// <summary>
    /// Options for <see cref="TKJobsModule"/>.
    /// </summary>
    public class TKJobsModuleOptions
    {
        /// <summary>
        /// 
        /// </summary>
        public ITKJobsService Service { get; set; }
    }
}