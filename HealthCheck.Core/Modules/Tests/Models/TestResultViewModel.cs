using System.Collections.Generic;

namespace HealthCheck.Core.Modules.Tests.Models
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
        /// Expand data in frontend by default.
        /// </summary>
        public bool ExpandDataByDefault { get; set; }

        /// <summary>
        /// Allow expanding the data in frontend. If false the expansion panel header will be hidden.
        /// </summary>
        public bool AllowExpandData { get; set; } = true;

        /// <summary>
        /// Removes expansion panel and copy/fullscreeen buttons.
        /// </summary>
        public bool DisplayClean { get; set; }

        /// <summary>
        /// Feedback per parameters.
        /// </summary>
        public Dictionary<int, string> ParameterFeedback { get; set; }

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
