using HealthCheck.Module.EndpointControl.Attributes;
using HealthCheck.Module.EndpointControl.Utils;
using System.Web.Http;

namespace HealthCheck.DevTest.Controllers.RequestLogTestControllers
{
    public class SomeApiTestController : ApiController
    {
        [Route("TestGetA")]
        [HttpGet]
        [HCControlledApiEndpoint]
        public string TestGetA(int id) => $"GET: {id}";

        [Route("TestPost")]
        [HttpPost]
        [HCControlledApiEndpoint]
        public string TestPost() => "POST";

        // http://localhost:32350/TestGetCustom
        [Route("TestGetCustom")]
        [HttpGet]
        [HCControlledApiEndpoint(CustomBlockedHandling = true)]
        public string TestGetCustom()
        {
            return $"GET | was blocked: {EndpointControlUtils.CurrentRequestWasDecidedBlocked()}";
        }
    }
}