using QoDL.Toolkit.Core.Config;
using QoDL.Toolkit.Core.Modules.Tests.Attributes;
using QoDL.Toolkit.Core.Modules.Tests.Models;

namespace QoDL.Toolkit.Dev.Common.Tests
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
            var typeName = TKGlobalConfig.GetDefaultInstanceResolver()?.ToString() ?? "<not set>";
            return TestResult.CreateSuccess($"Done! DefaultInstanceResolver is: {typeName}");
        }
    }
}
