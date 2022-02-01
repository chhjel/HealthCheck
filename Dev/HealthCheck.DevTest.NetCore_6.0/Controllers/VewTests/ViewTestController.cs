using HealthCheck.Core.Modules.Metrics.Context;
using HealthCheck.Dev.Common.Metrics;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace HealthCheck.DevTest.NetCore_6._0.Controllers.ViewTests
{
    [Route("[controller]")]
    public class ViewTestController : Controller
    {
        [HttpGet]
        public async Task<IActionResult> Index(bool error = false, bool metrics = true)
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
                    catch (Exception ex)
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
    }
}