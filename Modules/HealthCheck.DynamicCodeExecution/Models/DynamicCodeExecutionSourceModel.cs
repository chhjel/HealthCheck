using System.Collections.Generic;

namespace HealthCheck.DynamicCodeExecution.Models
{
    /// <summary>
    /// Model sent to the module from frontend.
    /// </summary>
    public class DynamicCodeExecutionSourceModel
    {
        /// <summary>
        /// The code to execute.
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// List of pre-processors to disable.
        /// </summary>
        public List<string> DisabledPreProcessorIds { get; set; }
    }
}
