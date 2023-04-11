using QoDL.Toolkit.Core.Modules.Tests.Attributes;
using QoDL.Toolkit.Core.Modules.Tests.Models;
using QoDL.Toolkit.Core.Util;
using System.Net;
using System.Threading.Tasks;

namespace QoDL.Toolkit.Dev.Common.Tests;

[RuntimeTestClass(
    GroupName = RuntimeTestConstants.Group.AdminStuff,
    DefaultRolesWithAccess = RuntimeTestAccessRole.SystemAdmins
)]
public class SomeServiceIntegrationTests
{
    [RuntimeTest(Name = "A test with a description and zero parameters", Description = "Some description here and a bit more text and just a bit more there thats it.")]
    public async Task<TestResult> TestWithDescriptionAndNoParams()
    {
        await Task.Delay(100);
        return TestResult.CreateSuccess($"Success, 124268724 items recieved!");
    }

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

    [RuntimeTest]
    public async Task<TestResult> SendAFailingWebRequestToGoogle()
    {
        var result = await TKConnectivityUtils.PerformWebRequestCheck("https://www.google.com/nonexistentpagehopefully");
        return result.ToTestResult();
    }

    [RuntimeTest]
    public async Task<TestResult> SendAFailingWebRequestToGoogleWithoutUtil()
    {
        await new WebClient().DownloadStringTaskAsync("https://www.google.com/nonexistentpagehopefully");
        return TestResult.CreateSuccess("Ok");
    }

    [RuntimeTest]
    public async Task<TestResult> SendAWebRequestToGoogle()
    {
        var result = await TKConnectivityUtils.PerformWebRequestCheck("https://www.google.com");
        return result.ToTestResult();
    }

    [RuntimeTest]
    public async Task<TestResult> PingGoogle()
    {
        var result = await TKConnectivityUtils.PingHost("google.com");
        return result.ToTestResult();
    }
}
