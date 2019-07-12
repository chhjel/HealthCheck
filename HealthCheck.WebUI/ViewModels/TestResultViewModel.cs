using HealthCheck.Core.Entities;
using HealthCheck.Core.Enums;
using System.Collections.Generic;

namespace HealthCheck.WebUI.ViewModels
{
    /// <summary>
    /// View model for a <see cref="TestResult"/>.
    /// </summary>
    public class TestResultViewModel
    {
        /// <summary>
        /// Id of the test this result is from.
        /// </summary>
        public string TestId { get; set; }

        /// <summary>
        /// name of the test this result is from.
        /// </summary>
        public string TestName { get; set; }

        /// <summary>
        /// Result status code.
        /// <para>0=<see cref="TestResultStatus.Success"/>, 1=<see cref="TestResultStatus.Warning"/>, 2=<see cref="TestResultStatus.Error"/></para>
        /// </summary>
        public int StatusCode { get; set; }

        /// <summary>
        /// Result status.
        /// </summary>
        public TestResultStatus Status { get; set; }

        /// <summary>
        /// Error/warning/success message.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Full stack trace of exception if any.
        /// </summary>
        public string StackTrace { get; set; }

        /// <summary>
        /// Any extra data.
        /// </summary>
        public List<TestResultDataDumpViewModel> Data { get; set; } = new List<TestResultDataDumpViewModel>();

        /// <summary>
        /// How long the text took to execute.
        /// </summary>
        public long DurationInMilliseconds { get; set; }

        /// <summary>
        /// Create a new <see cref="TestResultViewModel"/> with error status code.
        /// </summary>
        public static TestResultViewModel CreateError(string message, string testId = null, string testName = null)
        {
            return new TestResultViewModel()
            {
                TestId = testId,
                TestName = testName,
                StatusCode = (int)TestResultStatus.Error,
                Message = message
            };
        }
    }
}
