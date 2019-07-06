using HealthCheck.Core.Entities;
using HealthCheck.WebUI.Serializers;

namespace HealthCheck.WebUI.Extensions
{
    /// <summary>
    /// WebUI extensions for <see cref="TestResult"/>.
    /// </summary>
    public static class TestResultExtensions
    {
        private static readonly NewtonsoftJsonSerializer Serializer = new NewtonsoftJsonSerializer();

        /// <summary>
        /// Include a json serialized version of the given object in the result data.
        /// </summary>
        public static TestResult AddSerializedData(this TestResult testResut, object data, string title = null)
            => testResut.AddSerializedData(data, Serializer, title);
    }
}
