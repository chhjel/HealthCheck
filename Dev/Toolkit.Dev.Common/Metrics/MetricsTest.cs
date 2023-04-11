using QoDL.Toolkit.Core.Modules.Metrics.Context;
using System;
using System.Threading.Tasks;

namespace QoDL.Toolkit.Dev.Common.Metrics
{
    public class MetricsDummyService
    {
        public async Task<bool> Login()
        {
            if (!(await ValidateLogin()))
            {
                TKMetricsContext.AddNote("Login failed");
                TKMetricsContext.IncrementGlobalCounter("Failed login", 1);
                return false;
            }

            return true;
        }

        private async Task<bool> ValidateLogin()
        {
            TKMetricsContext.StartTiming("Login.Validate", "Validate login", addToGlobals: true);
            await Task.Delay(TimeSpan.FromSeconds(0.2));
            TKMetricsContext.EndTiming();
            return new Random().Next(100) > 50;
        }
    }
}
