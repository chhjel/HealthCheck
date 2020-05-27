using HealthCheck.Module.DynamicCodeExecution.Abstractions;

namespace HealthCheck.Module.DynamicCodeExecution.Models
{
    /// <summary>
    /// Item used to provide autocomplete.
    /// </summary>
    public class AutoCompleteData : IDynamicCodeCompletionData
    {
        /// <summary>
        /// Type of symbol.
        /// </summary>
        public string Kind { get; set; }

        /// <summary>
        /// Title for the autocomplete.
        /// </summary>
        public string Label { get; set; }

        /// <summary>
        /// Optional documentation.
        /// </summary>
        public string Documentation { get; set; }

        /// <summary>
        /// Text to insert if the user accepts the suggestion.
        /// </summary>
        public string InsertText { get; set; }
    }
}
