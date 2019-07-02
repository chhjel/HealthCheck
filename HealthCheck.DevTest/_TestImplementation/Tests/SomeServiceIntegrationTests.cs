using HealthCheck.Core.Attributes;
using HealthCheck.Core.Entities;
using System.Threading.Tasks;

namespace HealthCheck.DevTest._TestImplementation.Tests
{
    [RuntimeTestClass(
        GroupName = RuntimeTestConstants.Group.Test,
        DefaultRolesWithAccess = RuntimeTestAccessRole.SystemAdmins,
        Icon = RuntimeTestConstants.Icons.AspectRatio
    )]
    public class SomeServiceIntegrationTests
    {
        [RuntimeTest]
        public async Task<TestResult> TestServiceA(int id = 123, string orgName = "Test Organization", bool latestOnly = false, int someNumber = 42)
        {
            await Task.Delay(500);
            return TestResult.CreateSuccess($"Recieved [{id}, {orgName}, {latestOnly}, {someNumber}] and it was a success!");
        }

        [RuntimeTest]
        public async Task<TestResult> TestServiceB()
        {
            await Task.Delay(500);
            return TestResult.CreateSuccess($"Success, 124268724 items recieved!");
        }

        [RuntimeTest]
        public async Task<TestResult> TestServiceC()
        {
            await Task.Delay(1200);
            return TestResult.CreateSuccess($"Success, used 1200ms!");
        }

        [RuntimeTest]
        public async Task<TestResult> TestServiceD()
        {
            await Task.Delay(1100);
            return TestResult.CreateSuccess($"Success, it took about a second.");
        }
    }
}
