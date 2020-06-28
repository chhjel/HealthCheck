using HealthCheck.Module.DynamicCodeExecution.Abstractions;

namespace HealthCheck.Module.DynamicCodeExecution.AutoComplete.MCA.Models
{
    /// <summary>
    /// Suggestion for autocomplete.
    /// </summary>
    public class DynamicCodeExecutionAutoCompletionData : IDynamicCodeCompletionData
    {
        /// <summary>
        /// Monaco-style typename.
        /// </summary>
        public string Kind { get; set; }

        /// <summary>
        /// Title of the suggestion.
        /// </summary>
        public string Label { get; set; }

        /// <summary>
        /// Documentation for the item.
        /// </summary>
        public string Documentation { get; set; }

        /// <summary>
        /// Text that should be inserted if the suggestion is accepted.
        /// </summary>
        public string InsertText { get; set; }

        /// <summary>
        /// Suggestion for autocomplete.
        /// </summary>
        public DynamicCodeExecutionAutoCompletionData() { }

        /// <summary>
        /// Suggestion for autocomplete.
        /// </summary>
        public DynamicCodeExecutionAutoCompletionData(string kind, string label, string documentation, string insertText)
        {
            Kind = kind;
            Label = label;
            Documentation = documentation;
            InsertText = insertText;
        }

        /// <summary>
        /// Suggestion for autocomplete.
        /// </summary>
        public DynamicCodeExecutionAutoCompletionData(string kind, string label, string documentation, string xmlDoc, string insertText)
        {
            Kind = kind;
            Label = label;
            Documentation = documentation;
            InsertText = insertText;

            if (!string.IsNullOrWhiteSpace(xmlDoc))
            {
                Documentation = $"{xmlDoc}\n\n{Documentation}";
            }
        }
    }
}
