using QoDL.Toolkit.Core.Modules.Jobs.Services;
using System;

namespace QoDL.Toolkit.Core.Modules.Jobs.Attributes
{
    /// <summary>
    /// Used to provide some extra details to TK about this job.
    /// </summary>
    public class TKJobAttribute : Attribute
    {
        /// <summary>
        /// If set, inputs will be shown for this job and passed to <see cref="TKJobsService.StartJobAsync"/>.
        /// </summary>
        public Type CustomParametersType { get; set; }
    }
}
