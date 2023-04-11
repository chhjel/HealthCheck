using QoDL.Toolkit.Module.EndpointControl.Attributes;
using QoDL.Toolkit.Module.EndpointControl.Utils;
using Microsoft.AspNetCore.Mvc;

namespace QoDL.Toolkit.DevTest.NetCore_6._0.Controllers.EndpointControlTestControllers
{
    [Route("[controller]")]
    public class TestECController : Controller
    {
        [HttpGet]
        [TKControlledEndpoint]
        public IActionResult Index()
        {
            return Content("Index was here!");
        }

        [HttpGet]
        [Route("GetSomething")]
        [TKControlledEndpoint("Getty Somethingy")]
        public IActionResult GetSomething()
        {
            return Content("OK!");
        }

        [HttpPost]
        [Route("Submit")]
        [TKControlledEndpoint]
        public ActionResult Submit(string something)
        {
            return Content($"Posted {something}");
        }

        [HttpGet]
        [Route("GetSomethingCustom")]
        [TKControlledEndpoint(CustomBlockedHandling = true)]
        public ActionResult GetSomethingCustom()
        {
            return Content($"OK, maybe. | was blocked: {EndpointControlUtils.CurrentRequestWasDecidedBlocked()}");
        }
    }
}