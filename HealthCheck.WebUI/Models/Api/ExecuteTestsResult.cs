using HealthCheck.Core.Enums;
using System.Collections.Generic;
using System.Linq;

namespace HealthCheck.WebUI.Models.Api
{
    /// <summary>
    /// Result from the ExecuteTests endpoint.
    /// </summary>
    public class ExecuteTestsResult
    {
        /// <summary>
        /// Highest severity from the results.
        /// </summary>
        public TestResultStatus TotalResult { get; set; }

        /// <summary>
        /// Number of tests that executed successfully.
        /// </summary>
        public int SuccessCount => Results.Count(x => x.Result == TestResultStatus.Success);

        /// <summary>
        /// Number of tests that reported a warning.
        /// </summary>
        public int WarningCount => Results.Count(x => x.Result == TestResultStatus.Warning);

        /// <summary>
        /// Number of tests that failed.
        /// </summary>
        public int ErrorCount => Results.Count(x => x.Result == TestResultStatus.Error);

        /// <summary>
        /// Error if tests failed to start.
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// Result from all executed tests.
        /// </summary>
        public List<ExecuteTestsTestResult> Results { get; set; } = new List<ExecuteTestsTestResult>();
    }
}
