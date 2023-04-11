using QoDL.Toolkit.Core.Modules.Tests.Attributes;
using QoDL.Toolkit.Core.Modules.Tests.Models;
using System.Threading.Tasks;

namespace QoDL.Toolkit.Dev.Common.Tests
{
    [RuntimeTestClass(
        DefaultRolesWithAccess = RuntimeTestAccessRole.WebAdmins | RuntimeTestAccessRole.SystemAdmins,
        UIOrder = 100
    )]
    public class TestsForAdminsWithOneForGuests
    {
        [RuntimeTest]
        public async Task<TestResult> TestServiceA(int id = 123, string orgName = "Test Organization", bool latestOnly = false, int someNumber = 42)
        {
            await Task.Delay(500);
            return TestResult.CreateSuccess($"Recieved [{id}, {orgName}, {latestOnly}, {someNumber}] and it was a success!");
        }

        [RuntimeTest(RolesWithAccess = RuntimeTestAccessRole.Guest)]
        public async Task<TestResult> TestThatGuestsCanAccess()
        {
            await Task.Delay(500);
            return TestResult.CreateSuccess($"Success, 124268724 items recieved!");
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

    [RuntimeTestClass(
        DefaultRolesWithAccess = RuntimeTestAccessRole.WebAdmins | RuntimeTestAccessRole.SystemAdmins,
        UIOrder = 50,
        GroupName = RuntimeTestConstants.Group.BottomGroup
    )]
    public class TestSetA
    {
        [RuntimeTest]
        public async Task<TestResult> TestServiceX()
        {
            await Task.Delay(1200);
            return TestResult.CreateSuccess($"Success, used 1200ms!");
        }
    }

    [RuntimeTestClass(
        DefaultRolesWithAccess = RuntimeTestAccessRole.WebAdmins | RuntimeTestAccessRole.SystemAdmins,
        UIOrder = 0,
        GroupName = RuntimeTestConstants.Group.BottomGroup
    )]
    public class TestSetB
    {
        [RuntimeTest]
        public async Task<TestResult> TestServiceX()
        {
            await Task.Delay(1200);
            return TestResult.CreateSuccess($"Success, used 1200ms!");
        }
    }

    [RuntimeTestClass(
        Name = "Test Set C (slow ones)",
        DefaultRolesWithAccess = RuntimeTestAccessRole.WebAdmins | RuntimeTestAccessRole.SystemAdmins,
        UIOrder = -50,
        GroupName = RuntimeTestConstants.Group.BottomGroup
    )]
    public class TestSetC
    {
        [RuntimeTest] public async Task<TestResult> TestThatTakes2Seconds() => await CreateSlowResult(2000);
        [RuntimeTest] public async Task<TestResult> TestThatTakes4Seconds() => await CreateSlowResult(4000);
        [RuntimeTest] public async Task<TestResult> TestThatTakes6Seconds() => await CreateSlowResult(6000);
        [RuntimeTest] public async Task<TestResult> TestThatTakes8Seconds() => await CreateSlowResult(8000);
        [RuntimeTest] public async Task<TestResult> TestThatTakes10Seconds() => await CreateSlowResult(10000);

        private async Task<TestResult> CreateSlowResult(int delay)
        {
            await Task.Delay(delay);
            return TestResult.CreateSuccess($"Success!");
        }
    }

    [RuntimeTestClass(
        Name = "Test Set B",
        DefaultRolesWithAccess = RuntimeTestAccessRole.WebAdmins | RuntimeTestAccessRole.SystemAdmins,
        UIOrder = -100,
        GroupName = RuntimeTestConstants.Group.AdminStuff
    )]
    public class TestSetBDuplicatedName
    {
        [RuntimeTest]
        public async Task<TestResult> TestServiceX()
        {
            await Task.Delay(1200);
            return TestResult.CreateSuccess($"Success, used 1200ms!");
        }
    }
}
