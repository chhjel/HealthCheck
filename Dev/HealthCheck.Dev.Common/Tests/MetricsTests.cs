using HealthCheck.Core.Modules.Metrics.Context;
using HealthCheck.Core.Modules.Tests.Attributes;
using HealthCheck.Core.Modules.Tests.Models;
using System;
using System.Threading.Tasks;

namespace HealthCheck.Dev.Common.Tests
{
    [RuntimeTestClass(
        Name = "Metrics",
        DefaultRolesWithAccess = RuntimeTestAccessRole.WebAdmins,
        GroupName = RuntimeTestConstants.Group.Modules,
        UIOrder = 30
    )]
    public class MetricsTests
    {
        [RuntimeTest]
        public TestResult TrackSomeMetrics(int globalValue = 88, int noteValue = 1234)
        {
            TimeSpan timing = TimeSpan.FromSeconds(1.2);
            TimeSpan globalTiming = TimeSpan.FromSeconds(0.88);
            // todo: auto-include metrics if not already done.
            try
            {
                int.Parse("asd");
            }
            catch (Exception ex)
            {
                HCMetricsContext.AddError("Some error", ex);
            }

            HCMetricsContext.AddGlobalValue("num_retries", globalValue);
            HCMetricsContext.AddTiming("timing", "Some timing", timing, addToGlobals: false);
            HCMetricsContext.AddTiming("glob_timing", "Some global timing", globalTiming, addToGlobals: true);
            HCMetricsContext.AddNote("A note without value");
            HCMetricsContext.AddNote("A note with a value", noteValue);
            HCMetricsContext.AddGlobalNote("note1", $"Some global note 1 @ {DateTime.Now}");
            HCMetricsContext.IncrementGlobalCounter("glob_counter", 1);

            Task.Run(async () =>
            {
                await Task.Delay(200);

                HCMetricsContext.AddGlobalValue("static_num_retries", globalValue);
                HCMetricsContext.AddTiming("static_timing", "Some timing", timing, addToGlobals: false);
                HCMetricsContext.AddTiming("static_glob_timing", "Some global timing", globalTiming, addToGlobals: true);
                HCMetricsContext.AddNote("A static note without value");
                HCMetricsContext.AddNote("A static note with a value", noteValue);
                HCMetricsContext.AddGlobalNote("note2", $"Some static global note 2 @ {DateTime.Now}");
                HCMetricsContext.IncrementGlobalCounter("static_glob_counter", 1);
            });

            return TestResult.CreateSuccess("Some metrics should be tracked now.");
        }
    }
}
