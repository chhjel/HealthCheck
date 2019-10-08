namespace HealthCheck.Core.Modules.LogViewer.Enums
{
    /// <summary>
    /// Filter mode for search queries.
    /// </summary>
    public enum FilterQueryMode
    {
        /// <summary>
        /// Must match the input exactly.
        /// </summary>
        Exact = 0,

        /// <summary>
        /// Any of the words from the query must be present.
        /// </summary>
        AnyWord,

        /// <summary>
        /// All of the words from the query must be present.
        /// </summary>
        AllWords,

        /// <summary>
        /// Query is treated as a regex pattern.
        /// </summary>
        Regex
    }
}