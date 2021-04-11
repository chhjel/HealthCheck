using HealthCheck.Core.Modules.Metrics.Context;
using HealthCheck.Dev.Common.Metrics;
using System;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace HealthCheck.DevTest.Controllers.RequestLogTestControllers
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
                HCMetricsContext.StartTiming("Login", "Login", addToGlobals: true);
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
                        HCMetricsContext.AddError("Login error", ex);
                        HCMetricsContext.AddError("Another error");
                    }
                }
                HCMetricsContext.EndTiming("Login");

                HCMetricsContext.AddNote("Random value", new Random().Next());
                await Task.Delay(TimeSpan.FromSeconds(0.22));
                HCMetricsContext.AddNote("What just happened? 🤔");

                HCMetricsContext.AddGlobalValue("Rng", new Random().Next());
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