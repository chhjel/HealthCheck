using HealthCheck.Core.Modules.Dataflow.Abstractions;
using HealthCheck.Core.Modules.Dataflow.Services;
using System.Collections.Generic;

namespace HealthCheck.Core.Modules.Dataflow.Models
{
    /// <summary>
    /// Options for <see cref="DefaultDataflowService{TAccessRole}"/>.
    /// </summary>
    public class DefaultDataflowServiceOptions<TAccessRole>
    {
        /// <summary>
        /// Streams that returns data to display.
        /// </summary>
        public IEnumerable<IDataflowStream<TAccessRole>> Streams { get; set; }
    }
}
