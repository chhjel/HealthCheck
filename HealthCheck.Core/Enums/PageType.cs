namespace HealthCheck.Core.Enums
{
    /// <summary>
    /// Page types available.
    /// </summary>
    public enum HealthCheckPageType
    {
        /// <summary>
        /// The overview page with calendar and recent events.
        /// </summary>
        Overview = 0,

        /// <summary>
        /// Page where tests can be executed.
        /// </summary>
        Tests,

        /// <summary>
        /// Log of performed actions.
        /// </summary>
        AuditLog
    }
}
