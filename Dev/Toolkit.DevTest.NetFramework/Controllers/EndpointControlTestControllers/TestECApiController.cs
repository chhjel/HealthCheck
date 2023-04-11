using QoDL.Toolkit.Module.EndpointControl.Attributes;
using QoDL.Toolkit.Module.EndpointControl.Utils;
using System.Web.Http;

namespace QoDL.Toolkit.DevTest.Controllers.RequestLogTestControllers
{
    public class TestECApiController : ApiController
    {
        [Route("TestGetA")]
        [HttpGet]
        [TKControlledApiEndpoint]
        public string TestGetA(int id) => $"GET: {id}";

        [Route("TestPost")]
        [HttpPost]
        [TKControlledApiEndpoint]
        public string TestPost() => "POST";

        // http://localhost:32350/TestGetCustom?id=2
        [Route("TestGetCustom")]
        [HttpGet]
        [TKControlledApiEndpoint(CustomBlockedHandling = true, ManuallyCounted = true)]
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