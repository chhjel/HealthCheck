using HealthCheck.Module.EndpointControl.Attributes;
using HealthCheck.Module.EndpointControl.Utils;
using Microsoft.AspNetCore.Mvc;

namespace HealthCheck.DevTest.NetCore_3._1.Controllers.EndpointControlTestControllers
{
    [Route("[controller]")]
    public class TestECController : Controller
    {
        [HttpGet]
        [HCControlledEndpoint]
        public IActionResult Index()
        {
            return Content("Index was here!");
        }

        [HttpGet]
        [Route("GetSomething")]
        [HCControlledEndpoint("Getty Somethingy")]
        public IActionResult GetSomething()
        {
            return Content("OK!");
        }

        [HttpPost]
        [Route("Submit")]
        [HCControlledEndpoint]
        public ActionResult Submit(string something)
        {
            return Content($"Posted {something}");
        }

        [HttpGet]
        [Route("GetSomethingCustom")]
        [HCControlledEndpoint(CustomBlockedHandling = true)]
        public ActionResult GetSomethingCustom()
        {
            return Content($"OK, maybe. | was blocked: {EndpointControlUtils.CurrentRequestWasDecidedBlocked()}");
        }
    }
}