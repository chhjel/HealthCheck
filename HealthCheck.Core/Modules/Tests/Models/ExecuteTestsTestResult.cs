namespace HealthCheck.Core.Modules.Tests.Models
{
    /// <summary>
    /// Result from a single test.
    /// </summary>
    public class ExecuteTestsTestResult
    {
        /// <summary>
        /// Id of the executed test.
        /// </summary>
        public string TestId { get; set; }

        /// <summary>
        /// Name of the executed test.
        /// </summary>
        public string TestName { get; set; }

        /// <summary>
        /// Highest severity from the results.
        /// </summary>
        public TestResultStatus Result { get; set; }

        /// <summary>
        /// Test output.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Exception stacktrace if any.
        /// </summary>
        public string StackTrace { get; set; }
    }
}
