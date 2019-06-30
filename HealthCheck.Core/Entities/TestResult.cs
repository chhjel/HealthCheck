using HealthCheck.Core.Enums;
using HealthCheck.Core.Util;
using System.Collections.Generic;

namespace HealthCheck.Core.Entities
{
    /// <summary>
    /// Result from a test.
    /// </summary>
    public class TestResult
    {
        /// <summary>
        /// Result of the test.
        /// </summary>
        public TestResultStatus Status { get; set; }

        /// <summary>
        /// Test result message.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// The test that was executed.
        /// </summary>
        public TestDefinition Test { get; set; }

        /// <summary>
        /// Any optional data that will be stringified and returned with the result.
        /// </summary>
        public List<TestResultDataDump> Data { get; set; } = new List<TestResultDataDump>();

        /// <summary>
        /// Custom data object.
        /// </summary>
        public object Tag { get; set; }

        /// <summary>
        /// Create a new test success or error result depending on the given boolean value.
        /// </summary>
        /// <param name="success">True for success, false for failure.</param>
        /// <param name="message">Success or error message.</param>
        public static TestResult Create(bool success, string message)
            => Create(success ? TestResultStatus.Success : TestResultStatus.Error, message);

        /// <summary>
        /// Create a new test result with the given status.
        /// </summary>
        /// <param name="status">Test status.</param>
        /// <param name="message">Message text.</param>
        public static TestResult Create(TestResultStatus status, string message)
            => new TestResult() { Status = status, Message = message };

        /// <summary>
        /// Create a new successful test result.
        /// </summary>
        /// <param name="message">Message text with some extra details about the success.</param>
        public static TestResult CreateSuccess(string message)
            => Create(TestResultStatus.Success, message);

        /// <summary>
        /// Create a new warning test result.
        /// </summary>
        /// <param name="message">Message text with some extra details about the warning.</param>
        public static TestResult CreateWarning(string message)
            => Create(TestResultStatus.Warning, message);

        /// <summary>
        /// Create a new failed test result.
        /// </summary>
        /// <param name="message">Error text describing what went wrong.</param>
        public static TestResult CreateError(string message)
            => Create(TestResultStatus.Error, message);

        /// <summary>
        /// Include a serialized version of the given object in the result data.
        /// </summary>
        public TestResult AddSerializedData(object data, string title = null)
        {
            var dump = data.Dump(title);
            Data.Add(new TestResultDataDump()
            {
                Title = dump.Title,
                Data = dump.Data,
                IsJson = true
            });
            return this;
        }

        /// <summary>
        /// Include the given text content in the result data.
        /// </summary>
        public TestResult AddData(string data, string title = null)
        {
            Data.Add(new TestResultDataDump()
            {
                Title = title,
                Data = data
            });
            return this;
        }
    }
}
