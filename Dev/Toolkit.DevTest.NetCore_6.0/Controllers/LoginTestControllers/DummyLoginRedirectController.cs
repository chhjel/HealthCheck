using Microsoft.AspNetCore.Mvc;

namespace QoDL.Toolkit.DevTest.NetCore_6._0.Controllers.LoginTestControllers;

[Route("[controller]")]
public class DummyLoginRedirectController : Controller
{
    [HttpGet]
    public IActionResult Index([FromQuery] string r)
    {
        System.Diagnostics.Debug.WriteLine($"Redirect: {r}");
        return Redirect(r);
    }
}
