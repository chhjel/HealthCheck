using System.Collections.Generic;

namespace HealthCheck.Core.Modules.Dataflow
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
