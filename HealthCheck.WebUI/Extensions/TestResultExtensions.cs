﻿using HealthCheck.Core.Modules.Tests.Models;
using HealthCheck.Core.Modules.Tests.Utils.HtmlPresets;
using HealthCheck.WebUI.Serializers;

namespace HealthCheck.WebUI.Extensions
{
    /// <summary>
    /// WebUI extensions for <see cref="TestResult"/>.
    /// </summary>
    public static class TestResultExtensions
    {
        private static readonly NewtonsoftJsonSerializer _serializer = new NewtonsoftJsonSerializer();

        /// <summary>
        /// Include a json serialized version of the given object in the result data.
        /// </summary>
        public static TestResult AddSerializedData(this TestResult testResult, object data, string title = null)
            => testResult.AddSerializedData(data, _serializer, title);

        /// <summary>
        /// Include html preset data.
        /// </summary>
        public static TestResult AddHtmlData(this TestResult testResult, HtmlPresetBuilder htmlBuilder, string title = null, bool onlyIfNotNullOrEmpty = true)
            => testResult.AddHtmlData(htmlBuilder?.ToHtml(), title, onlyIfNotNullOrEmpty);
    }
}
