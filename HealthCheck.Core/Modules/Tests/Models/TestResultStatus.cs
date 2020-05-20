namespace HealthCheck.Core.Modules.Tests.Models
{
    /// <summary>
    /// Test result status.
    /// </summary>
    public enum TestResultStatus
    {
        /// <summary>
        /// Test result was a success.
        /// </summary>
        Success = 0,

        /// <summary>
        /// Result had some non-critical issues
        /// </summary>
        Warning,

        /// <summary>
        /// Test failed.
        /// </summary>
        Error
    }
}
