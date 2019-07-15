namespace HealthCheck.Core.Enums
{
    /// <summary>
    /// Type of data dump.
    /// </summary>
    public enum TestResultDataDumpType
    {
        /// <summary>
        /// Plain text.
        /// </summary>
        PlainText = 0,

        /// <summary>
        /// Json data.
        /// </summary>
        Json,

        /// <summary>
        /// Xml data.
        /// </summary>
        Xml,

        /// <summary>
        /// Html data.
        /// </summary>
        Html,

        /// <summary>
        /// Image urls. Separate with newlines if done manually.
        /// </summary>
        ImageUrls,

        /// <summary>
        /// Urls. Separate with newlines if done manually.
        /// </summary>
        Urls
    }
}
