using HealthCheck.Module.EndpointControl.Attributes;
using HealthCheck.Module.EndpointControl.Utils;
using System.Web.Http;

namespace HealthCheck.DevTest.Controllers.RequestLogTestControllers
{
    public class TestECApiController : ApiController
    {
        [Route("TestGetA")]
        [HttpGet]
        [HCControlledApiEndpoint]
        public string TestGetA(int id) => $"GET: {id}";

        [Route("TestPost")]
        [HttpPost]
        [HCControlledApiEndpoint]
        public string TestPost() => "POST";

        // http://localhost:32350/TestGetCustom?id=2
        [Route("TestGetCustom")]
        [HttpGet]
        [HCControlledApiEndpoint(CustomBlockedHandling = true, ManuallyCounted = true)]
        public string TestGetCustom(int id = 1)
        {
            var counted = false;
            if (id == 2)
            {
                counted = true;
                EndpointControlUtils.CountCurrentRequest();
            }
            return $"GET | was blocked: {EndpointControlUtils.CurrentRequestWasDecidedBlocked()} | was counted: {counted}";
        }
    }
}