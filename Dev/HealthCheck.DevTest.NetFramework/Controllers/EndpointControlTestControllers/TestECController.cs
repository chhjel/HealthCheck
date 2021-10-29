using HealthCheck.Module.EndpointControl.Attributes;
using HealthCheck.Module.EndpointControl.Utils;
using System.Web.Mvc;

namespace HealthCheck.DevTest.Controllers.RequestLogTestControllers
{
    public class TestECController : Controller
    {
        [HCControlledEndpoint("Getty Somethingy")]
        public ActionResult GetSomething()
        {
            return Content("OK!");
        }

        [HttpPost]
        [HCControlledEndpoint]
        public ActionResult Submit(string something)
        {
            return View();
        }

        [HttpGet]
        [HCControlledEndpoint(CustomBlockedHandling = true)]
        public ActionResult GetSomethingCustom()
        {
            return Content($"OK, maybe. | was blocked: {EndpointControlUtils.CurrentRequestWasDecidedBlocked()}");
        }
    }
}