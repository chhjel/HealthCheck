using System.Collections.Generic;

namespace HealthCheck.Module.DynamicCodeExecution.Models
{
    /// <summary>
    /// Request to be sent to the endpoint.
    /// </summary>
    public class CompletionRequest
    {
        /// <summary>
        /// All the code
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// Cursor position
        /// </summary>
        public int Position { get; set; }
    }
}
