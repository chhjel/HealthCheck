using QoDL.Toolkit.Core.Modules.Metrics.Context;
using QoDL.Toolkit.Dev.Common.Metrics;
using System;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace QoDL.Toolkit.DevTest.Controllers.RequestLogTestControllers
{
    public class HomeController : Controller
    {
        public async Task<ActionResult> Index(bool error = false, bool metrics = true)
        {
            if (error)
            {
                DateTimeOffset.Parse("nope");
            }
            if (metrics)
            {
                TKMetricsContext.StartTiming("Login", "Login", addToGlobals: true);
                await Task.Delay(TimeSpan.FromSeconds(0.1));
                var service = new MetricsDummyService();
                if (!(await service.Login()))
                {
                    await Task.Delay(TimeSpan.FromSeconds(0.15));

                    try
                    {
                        _ = int.Parse("err pls");
                    }
                    catch(Exception ex)
                    {
                        TKMetricsContext.AddError("Login error", ex);
                        TKMetricsContext.AddError("Another error");
                    }
                }
                TKMetricsContext.EndTiming("Login");

                TKMetricsContext.AddNote("Random value", new Random().Next());
                await Task.Delay(TimeSpan.FromSeconds(0.22));
                TKMetricsContext.AddNote("What just happened? 🤔");

                TKMetricsContext.AddGlobalValue("Rng", new Random().Next());
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