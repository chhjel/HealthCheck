using QoDL.Toolkit.Core.Modules.Tests.Attributes;
using QoDL.Toolkit.Core.Modules.Tests.Models;
using System.Threading.Tasks;

namespace QoDL.Toolkit.Dev.Common.Tests
{
    [RuntimeTestClass(
        DefaultRolesWithAccess = RuntimeTestAccessRole.WebAdmins | RuntimeTestAccessRole.API,
        UIOrder = -200,
        DefaultCategory = RuntimeTestConstants.Categories.APIChecks,
        DefaultAllowParallelExecution = true
    )]
    public class SomeAPITests
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
            return TestResult.CreateWarning($"Success, it took about a second.");
        }

        [RuntimeTest]
        public async Task<TestResult> ASlowTest()
        {
            await Task.Delay(3000);
            return TestResult.CreateSuccess($"Success, it took about 3 seconds.");
        }

        [RuntimeTest]
        public async Task<TestResult> TestSomething()
        {
            await Task.Delay(1100);
            int.Parse("1234$");
            return TestResult.CreateError($"Total failure!");
        }
    }
}
