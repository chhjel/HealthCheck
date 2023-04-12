using QoDL.Toolkit.Module.EndpointControl.Attributes;
using QoDL.Toolkit.Module.EndpointControl.Utils;
using System.Web.Mvc;

namespace QoDL.Toolkit.DevTest.Controllers.RequestLogTestControllers;

public class TestECController : Controller
{
    [TKControlledEndpoint("Getty Somethingy")]
    public ActionResult GetSomething()
    {
        return Content("OK!");
    }

    [HttpPost]
    [TKControlledEndpoint]
    public ActionResult Submit(string something)
    {
        return View();
    }

    [HttpGet]
    [TKControlledEndpoint(CustomBlockedHandling = true)]
    public ActionResult GetSomethingCustom()
    {
        return Content($"OK, maybe. | was blocked: {EndpointControlUtils.CurrentRequestWasDecidedBlocked()}");
    }
}