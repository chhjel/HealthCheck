using QoDL.Toolkit.Core.Modules.Metrics.Context;
using QoDL.Toolkit.Core.Modules.Tests.Attributes;
using QoDL.Toolkit.Core.Modules.Tests.Models;
using QoDL.Toolkit.Core.Util;
using System;
using System.Threading.Tasks;

namespace QoDL.Toolkit.Dev.Common.Tests.Modules;

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
            TKMetricsContext.AddError("Some error", ex);
            TKMetricsContext.AddGlobalNote("err_1", TKExceptionUtils.GetFullExceptionDetails(ex));
        }

        TKMetricsContext.AddGlobalValue("num_retries", globalValue);
        TKMetricsContext.AddTiming("timing", "Some timing", timing, addToGlobals: false);
        TKMetricsContext.AddTiming("glob_timing", "Some global timing", globalTiming ?? TimeSpan.Zero, addToGlobals: true);
        TKMetricsContext.AddNote("A note without value");
        TKMetricsContext.AddNote("A note with a value", noteValue);
        TKMetricsContext.AddGlobalNote("note1", $"Some global note 1 @ {DateTime.Now}");
        TKMetricsContext.IncrementGlobalCounter("glob_counter", 1);

        Task.Run(async () =>
        {
            await Task.Delay(200);

            TKMetricsContext.AddGlobalValue("static_num_retries", globalValue);
            TKMetricsContext.AddTiming("static_timing", "Some timing", timing, addToGlobals: false);
            TKMetricsContext.AddTiming("static_glob_timing", "Some global timing", globalTiming ?? TimeSpan.Zero, addToGlobals: true);
            TKMetricsContext.AddNote("A static note without value");
            TKMetricsContext.AddNote("A static note with a value", noteValue);
            TKMetricsContext.AddGlobalNote("note2", $"Some static global note 2 @ {DateTime.Now}");
            TKMetricsContext.IncrementGlobalCounter("static_glob_counter", 1);
        });

        return TestResult.CreateSuccess("Some metrics should be tracked now.");
    }

    public static TimeSpan TimingDefault() => TimeSpan.FromSeconds(1.2);
}
