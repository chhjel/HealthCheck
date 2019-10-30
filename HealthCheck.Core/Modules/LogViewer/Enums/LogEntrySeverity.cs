namespace HealthCheck.Core.Modules.LogViewer.Enums
{
    /// <summary>
    /// Severity of a log entry.
    /// </summary>
    public enum LogEntrySeverity
    {
        /// <summary>
        /// Below warning-level.
        /// </summary>
        Info = 0,

        /// <summary>
        /// Warning level.
        /// </summary>
        Warning,

        /// <summary>
        /// Error-level or above.
        /// </summary>
        Error
    }
}
