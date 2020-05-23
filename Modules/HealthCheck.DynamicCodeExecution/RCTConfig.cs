using HealthCheck.DynamicCodeExecution.Abstractions;
using System.Collections.Generic;

namespace HealthCheck.DynamicCodeExecution
{
    /// <summary>
    /// Configuration for the runtime code tester.
    /// </summary>
    internal class RCTConfig
    {
        /// <summary>
        /// Allow the autocomplete endpoint to delegate the task by sending the request to the given endpoint.
        /// </summary>
        public bool AllowAutoCompleteDelegation { get; set; } = true;

        /// <summary>
        /// List of pre-processors to run the code through before execution.
        /// </summary>
        public IEnumerable<IDynamicCodePreProcessor> PreProcessors { get; set; }

        /// <summary>
        /// Optional auto-completer.
        /// </summary>
        public IDynamicCodeAutoCompleter AutoComplete { get; set; }
    }
}
