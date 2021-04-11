using HealthCheck.Core.Modules.Metrics.Context;
using System;
using System.Threading.Tasks;

namespace HealthCheck.Dev.Common.Metrics
{
    public class MetricsDummyService
    {
        public async Task<bool> Login()
        {
            if (!(await ValidateLogin()))
            {
                HCMetricsContext.AddNote("Login failed");
                HCMetricsContext.IncrementGlobalCounter("Failed login", 1);
                return false;
            }

            return true;
        }

        private async Task<bool> ValidateLogin()
        {
            HCMetricsContext.StartTiming("Login.Validate", "Validate login", addToGlobals: true);
            await Task.Delay(TimeSpan.FromSeconds(0.2));
            HCMetricsContext.EndTiming();
            return new Random().Next(100) > 50;
        }
    }
}
