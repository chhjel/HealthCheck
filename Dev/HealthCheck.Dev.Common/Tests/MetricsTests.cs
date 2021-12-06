using HealthCheck.Core.Modules.Metrics.Context;
using HealthCheck.Core.Modules.Tests.Attributes;
using HealthCheck.Core.Modules.Tests.Models;
using HealthCheck.Core.Util;
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
        [RuntimeTestParameter(target: "timing", "Timing", null, DefaultValueFactoryMethod = nameof(TimingDefault))]
        public TestResult TrackSomeMetrics(TimeSpan timing, TimeSpan? globalTiming = null, int globalValue = 88, int noteValue = 1234)
        {
            try
            {
                int.Parse("asd");
            }
            catch (Exception ex)
            {
                HCMetricsContext.AddError("Some error", ex);
                HCMetricsContext.AddGlobalNote("err_1", ExceptionUtils.GetFullExceptionDetails(ex));
            }

            HCMetricsContext.AddGlobalValue("num_retries", globalValue);
            HCMetricsContext.AddTiming("timing", "Some timing", timing, addToGlobals: false);
            HCMetricsContext.AddTiming("glob_timing", "Some global timing", globalTiming ?? TimeSpan.Zero, addToGlobals: true);
            HCMetricsContext.AddNote("A note without value");
            HCMetricsContext.AddNote("A note with a value", noteValue);
            HCMetricsContext.AddGlobalNote("note1", $"Some global note 1 @ {DateTime.Now}");
            HCMetricsContext.IncrementGlobalCounter("glob_counter", 1);

            Task.Run(async () =>
            {
                await Task.Delay(200);

                HCMetricsContext.AddGlobalValue("static_num_retries", globalValue);
                HCMetricsContext.AddTiming("static_timing", "Some timing", timing, addToGlobals: false);
                HCMetricsContext.AddTiming("static_glob_timing", "Some global timing", globalTiming ?? TimeSpan.Zero, addToGlobals: true);
                HCMetricsContext.AddNote("A static note without value");
                HCMetricsContext.AddNote("A static note with a value", noteValue);
                HCMetricsContext.AddGlobalNote("note2", $"Some static global note 2 @ {DateTime.Now}");
                HCMetricsContext.IncrementGlobalCounter("static_glob_counter", 1);
            });

            return TestResult.CreateSuccess("Some metrics should be tracked now.");
        }

        public static TimeSpan TimingDefault() => TimeSpan.FromSeconds(1.2);
    }
}
