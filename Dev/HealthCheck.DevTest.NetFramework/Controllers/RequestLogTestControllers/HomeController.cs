using System;
using System.Web.Mvc;

namespace HealthCheck.DevTest.Controllers.RequestLogTestControllers
{
    public class HomeController : Controller
    {
        public ActionResult Index(bool error=false)
        {
            if (error)
            {
                DateTimeOffset.Parse("nope");
            }
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            var error = int.Parse("nope");
            return View(error);
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return HttpNotFound();
        }

        [HttpPost]
        public ActionResult HomePostTest()
        {
            return Content("OK!");
        }
    }
}