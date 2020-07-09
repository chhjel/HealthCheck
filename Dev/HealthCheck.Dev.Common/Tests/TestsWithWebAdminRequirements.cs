using HealthCheck.Core.Modules.Tests.Attributes;
using HealthCheck.Core.Modules.Tests.Models;
using System.Threading.Tasks;

namespace HealthCheck.Dev.Common.Tests
{
    [RuntimeTestClass(
        DefaultRolesWithAccess = RuntimeTestAccessRole.WebAdmins
    )]
    public class TestsWithWebAdminRequirements
    {
        [RuntimeTest]
        public async Task<TestResult> TestServiceA(int id = 123, string orgName = "Test Organization", bool latestOnly = false, int someNumber = 42)
        {
            await Task.Delay(500);
            return TestResult.CreateSuccess($"Recieved [{id}, {orgName}, {latestOnly}, {someNumber}] and it was a success!");
        }

        [RuntimeTest]
        public async Task<TestResult> TestServiceX()
        {
            await Task.Delay(1200);
            return TestResult.CreateSuccess($"Success, used 1200ms!");
        }

        [RuntimeTest]
        public async Task<TestResult> TestServiceY()
        {
            await Task.Delay(1100);
            return TestResult.CreateSuccess($"Success, it took about a second.");
        }
    }
}
