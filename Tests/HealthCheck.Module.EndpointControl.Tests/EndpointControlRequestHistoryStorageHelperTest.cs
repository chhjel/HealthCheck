using HealthCheck.Module.EndpointControl.Models;
using HealthCheck.Module.EndpointControl.Utils;
using Xunit;

namespace HealthCheck.Module.EndpointControl.Tests
{
    public class EndpointControlRequestHistoryStorageHelperTest
    {
        [Fact]
        public void AddRequestToCollections_WithExpectedData_DoesNotThrow()
        {
            var helper = new EndpointControlRequestHistoryStorageHelper();
            var request = CreateRequestData();
            helper.AddRequestToCollections(request);
        }

        [Fact]
        public void AddRequestToCollections_Scenario1_DoesNotThrow()
        {
            var helper = new EndpointControlRequestHistoryStorageHelper();
            var request = CreateRequestData(ip: "0.0.0.0", method: "POST", url: "https://something.no/HealthCheckLogin/Login");
            helper.AddRequestToCollections(request);
        }

        [Theory]
        [InlineData(null, "TestAgent", "GET", "https://test.com")]
        [InlineData("127.0.0.1", null, "GET", "https://test.com")]
        [InlineData("127.0.0.1", "TestAgent", null, "https://test.com")]
        [InlineData("127.0.0.1", "TestAgent", "GET", null)]
        public void AddRequestToCollections_WithNullValue_DoesNotThrow(string ip, string userAgent, string method, string url)
        {
            var helper = new EndpointControlRequestHistoryStorageHelper();
            var request = CreateRequestData(ip, userAgent, method, url);
            helper.AddRequestToCollections(request);
        }

        [Theory]
        [InlineData("", "TestAgent", "GET", "https://test.com")]
        [InlineData("127.0.0.1", "", "GET", "https://test.com")]
        [InlineData("127.0.0.1", "TestAgent", "", "https://test.com")]
        [InlineData("127.0.0.1", "TestAgent", "GET", "")]
        public void AddRequestToCollections_WithEmptyValue_DoesNotThrow(string ip, string userAgent, string method, string url)
        {
            var helper = new EndpointControlRequestHistoryStorageHelper();
            var request = CreateRequestData(ip, userAgent, method, url);
            helper.AddRequestToCollections(request);
        }

        private static EndpointControlEndpointRequestData CreateRequestData(string ip = "127.0.0.1", string userAgent = "TestAgent",
            string method = "GET", string url = "https://test.com")
        {
            var data = new EndpointControlEndpointRequestData
            {
                ControllerType = typeof(EndpointControlEndpointRequestData),
                ControllerName = "TestController",
                ActionName = "TestAction",

                UserLocationId = ip,
                UserAgent = userAgent,
                HttpMethod = method,
                Url = url,
            };

            data.EndpointName = $"{data.ControllerName?.Replace("Controller", "")}.{data.ActionName} ({data.HttpMethod})";
            data.EndpointId = $"{data.ControllerName?.Replace("Controller", "")}|{data.ActionName}|{data.HttpMethod}";
            return data;
        }
    }
}