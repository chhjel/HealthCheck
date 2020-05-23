using System.Collections.Generic;

namespace HealthCheck.DynamicCodeExecution.Models
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
        /// Assembly locations to get data from
        /// </summary>
        public IEnumerable<string> AssemblyLocations { get; set; }

        /// <summary>
        /// Cursor position
        /// </summary>
        public int Position { get; set; }

        /// <summary>
        /// Optional endpoint to use instead of the configured autocompleter.
        /// </summary>
        public string DelegateTo { get; set; }
    }
}
