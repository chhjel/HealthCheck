using System.Collections.Generic;

namespace HealthCheck.Core.Modules.Dataflow
{
    /// <summary>
    /// Options for <see cref="DefaultDataflowService"/>.
    /// </summary>
    public class DefaultDataflowServiceOptions
    {
        /// <summary>
        /// Streams that returns data to display.
        /// </summary>
        public IEnumerable<IDataflowStream> Streams { get; set; }
    }

}
