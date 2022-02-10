using HealthCheck.Core.Modules.Tests.Models;
using System;

namespace HealthCheck.Core.Util.Models
{
    /// <summary>
    /// Result from connectivity utility methods.
    /// </summary>
    public class HCConnectivityCheckResult
    {
        /// <summary>
        /// True if the check was a success.
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Summary of the action that was performed.
        /// </summary>
        public string Action { get; set; }

        /// <summary>
        /// Any extra details about the success if any.
        /// </summary>
        public string SuccessDetails { get; set; }

        /// <summary>
        /// Short description of the error if any.
        /// </summary>
        public string Error { get; set; }

        /// <summary>
        /// Error details if there was any error.
        /// </summary>
        public string Details { get; set; }

        /// <summary>
        /// Exception if any.
        /// </summary>
        public Exception Exception { get; set; }

        /// <summary>
        /// Create a new error result.
        /// </summary>
        public static HCConnectivityCheckResult CreateError(string action, string error, string details = null)
        {
            return new HCConnectivityCheckResult()
            {
                Success = false,
                Action = action,
                Error = error,
                Details = details
            };
        }

        /// <summary>
        /// Create a new error result from the given exception.
        /// </summary>
        public static HCConnectivityCheckResult CreateError(string action, Exception exception)
        {
            return new HCConnectivityCheckResult()
            {
                Success = false,
                Action = action,
                Error = HCExceptionUtils.GetExceptionSummary(exception),
                Details = HCExceptionUtils.GetExceptionDetails(exception),
                Exception = exception
            };
        }

        /// <summary>
        /// Create a new success result.
        /// </summary>
        public static HCConnectivityCheckResult CreateSuccess(string action, string details = null)
        {
            return new HCConnectivityCheckResult()
            {
                Success = true,
                Action = action,
                SuccessDetails = details
            };
        }

        /// <summary>
        /// Create a new <see cref="TestResult"/> from this object.
        /// </summary>
        /// <param name="nonSuccessIsWarning">Non-success will be returned as a warning if true.</param>
        /// <param name="includeDetails">Include any details in <see cref="Details"/> as extra result data.</param>
        public TestResult ToTestResult(bool nonSuccessIsWarning = false, bool includeDetails = true)
        {
            TestResult result;
            if (Success)
            {
                result = TestResult.CreateSuccess(SuccessDetails);
            }
            else
            {
                result = TestResult.Create(nonSuccessIsWarning ? TestResultStatus.Warning : TestResultStatus.Error, Error, Exception);
            }

            if (includeDetails && !string.IsNullOrWhiteSpace(Details))
            {
                result.AddTextData("Details", Details);
            }
            return result;
        }
    }
}
