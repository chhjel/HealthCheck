using Microsoft.AspNetCore.Mvc;

namespace HealthCheck.DevTest.NetCore_3._1.Controllers
{
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
}
