namespace HealthCheck.Module.DynamicCodeExecution.Abstractions
{
    /// <summary>
    /// Data returned from <see cref="IDynamicCodeAutoCompleter"/>.
    /// </summary>
    public interface IDynamicCodeCompletionData
    {
        /// <summary>
        /// Type of data we are completing.
        /// </summary>
        string Kind { get; }

        /// <summary>
        /// Label of the autocomplete data.
        /// </summary>
        string Label { get; }

        /// <summary>
        /// Documentation if available.
        /// </summary>
        string Documentation { get; }

        /// <summary>
        /// The text to insert if the user choses to.
        /// </summary>
        string InsertText { get; }
    }
}
