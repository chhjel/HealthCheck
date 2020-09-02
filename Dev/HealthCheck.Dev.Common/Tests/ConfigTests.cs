using HealthCheck.Core.Config;
using HealthCheck.Core.Modules.Tests.Attributes;
using HealthCheck.Core.Modules.Tests.Models;

namespace HealthCheck.Dev.Common.Tests
{
    [RuntimeTestClass(
        DefaultRolesWithAccess = RuntimeTestAccessRole.WebAdmins,
        UIOrder = -300,
        DefaultAllowParallelExecution = true
    )]
    public class ConfigTests
    {
        [RuntimeTest]
        public TestResult InvokeConfig()
        {
            var typeName = HCGlobalConfig.GetDefaultInstanceResolver()?.ToString() ?? "<not set>";
            return TestResult.CreateSuccess($"Done! DefaultInstanceResolver is: {typeName}");
        }
    }
}
