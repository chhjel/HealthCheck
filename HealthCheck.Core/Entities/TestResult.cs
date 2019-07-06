﻿using HealthCheck.Core.Abstractions;
using HealthCheck.Core.Enums;
using HealthCheck.Core.Util;
using System;
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
        /// Full stack trace of exception if any.
        /// </summary>
        public string StackTrace { get; set; }

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
        /// Create a new test result with the given status.
        /// </summary>
        /// <param name="status">Test status.</param>
        /// <param name="message">Message text. If null exception message will be used if any.</param>
        /// <param name="exception">Exception if any to get stack trace from.</param>
        public static TestResult Create(TestResultStatus status, string message, Exception exception = null)
            => new TestResult()
            {
                Status = status,
                Message = message ?? exception?.Message,
                StackTrace = exception?.ToString()
            };

        /// <summary>
        /// Create a new test success or error result depending on the given boolean value.
        /// </summary>
        /// <param name="success">True for success, false for failure.</param>
        /// <param name="message">Success or error message. If null exception message will be used if any.</param>
        /// <param name="exception">Exception if any to get stack trace from.</param>
        public static TestResult Create(bool success, string message, Exception exception = null)
            => Create(success ? TestResultStatus.Success : TestResultStatus.Error, message, exception);

        /// <summary>
        /// Create a new successful test result.
        /// </summary>
        /// <param name="message">Message text with some extra details about the success.</param>
        public static TestResult CreateSuccess(string message)
            => Create(TestResultStatus.Success, message);

        /// <summary>
        /// Create a new warning test result.
        /// </summary>
        /// <param name="message">Message text with some extra details about the warning. If null exception message will be used if any.</param>
        /// <param name="exception">Exception if any to get stack trace from.</param>
        public static TestResult CreateWarning(string message, Exception exception = null)
            => Create(TestResultStatus.Warning, message, exception);

        /// <summary>
        /// Create a new failed test result.
        /// </summary>
        /// <param name="message">Error text describing what went wrong. If null exception message will be used if any.</param>
        /// <param name="exception">Exception if any to get stack trace from.</param>
        public static TestResult CreateError(string message, Exception exception = null)
            => Create(TestResultStatus.Error, message, exception);

        #region Method-chaining
        /// <summary>
        /// Include a serialized version of the given object in the result data.
        /// <para>If using HealthCheck.WebUI the NewtonsoftJsonSerializer() or just use the AddSerializedData(object data, string title=null) extension method from HealthCheck.WebUI.</para>
        /// </summary>
        public TestResult AddSerializedData(object data, IDumpJsonSerializer serializer, string title = null)
        {
            var dump = data.Dump(serializer, title);
            Data.Add(new TestResultDataDump()
            {
                Title = dump.Title,
                Data = dump.Data,
                Type = TestResultDataDumpType.Json
            });
            return this;
        }

        /// <summary>
        /// Include the given content in the result data.
        /// </summary>
        public TestResult AddData(string data, string title = null, TestResultDataDumpType type = TestResultDataDumpType.PlainText)
        {
            Data.Add(new TestResultDataDump()
            {
                Title = title,
                Data = data,
                Type = type
            });
            return this;
        }

        /// <summary>
        /// Include the given html in the result data.
        /// </summary>
        public TestResult AddTextData(string text, string title = null)
            => AddData(text, title, TestResultDataDumpType.PlainText);

        /// <summary>
        /// Include the given html in the result data.
        /// </summary>
        public TestResult AddHtmlData(string html, string title = null)
            => AddData(html, title, TestResultDataDumpType.Html);

        /// <summary>
        /// Include the given xml in the result data.
        /// </summary>
        public TestResult AddXmlData(string xml, string title = null)
            => AddData(xml, title, TestResultDataDumpType.Xml);

        /// <summary>
        /// Include the given json in the result data.
        /// </summary>
        public TestResult AddJsonData(string json, string title = null)
            => AddData(json, title, TestResultDataDumpType.Json);

        /// <summary>
        /// Include the given image urls in the result data.
        /// </summary>
        public TestResult AddImageUrlsData(IEnumerable<string> urls, string title = null)
            => AddData(string.Join(Environment.NewLine, urls ?? new string[0]), title, TestResultDataDumpType.ImageUrls);
        #endregion
    }
}
