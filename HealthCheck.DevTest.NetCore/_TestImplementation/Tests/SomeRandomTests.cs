using HealthCheck.Core.Attributes;
using HealthCheck.Core.Entities;
using HealthCheck.WebUI.Serializers;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace HealthCheck.DevTest._TestImplementation.Tests
{
    [RuntimeTestClass(
        Name = "Some fancy tests in .Net Core",
        Description = "Some fancy description",
        DefaultRolesWithAccess = RuntimeTestAccessRole.SystemAdmins,
        GroupName = RuntimeTestConstants.Group.AdminStuff
    )]
    public class SomeRandomTests
    {
        [RuntimeTest(
            Name = "Get data from X",
            Description = "Retrieve some data from some service."
        )]
        [RuntimeTestParameter("id", "Data id", "Id of the thing to get")]
        [RuntimeTestParameter("orgName", "Organization name", "Name of the organization the data belongs to")]
        [RuntimeTestParameter("latestOnly", "Only get the latest data", "If true only the latest data will be retrieved.")]
        public async Task<TestResult> GetDataFromX(int id = 123, string orgName = "Test Organization", bool latestOnly = false, int someNumber = 42)
        {
            await Task.Delay(300);
            return TestResult.CreateSuccess($"Recieved [{id}, {orgName}, {latestOnly}, {someNumber}] and it was a success!");
        }

        [RuntimeTest("Html result test")]
        public TestResult SomeFancyTest([RuntimeTestParameter("H-Number", "What H-tag to write")] int hNumber = 3, string text = "Title")
        {
            return TestResult.CreateSuccess($"Success")
                .AddHtmlData($"This is the h-tag you ordered: <h{hNumber}>{text}</h{hNumber}>");
        }

        [RuntimeTest]
        public async Task<TestResult> TestThatThrowsException()
        {
            await Task.Delay(100);
            throw new System.ArgumentOutOfRangeException("nonExistentArgument");
        }

        [RuntimeTest]
        public async Task<TestResult> TestThatReturnsErrorWithExceptionStackTrace()
        {
            try
            {
                await Task.Delay(100);
                throw new System.ArgumentOutOfRangeException("nonExistentArgument");
            }
            catch (Exception ex)
            {
                return TestResult.CreateError("Result with exception included", ex);
            }
        }

        [RuntimeTest(
            Name = "Custom name for this one",
            Description = "Fancy fancy description"
        )]
        public TestResult SomeFancyTest(int number = 123)
        {
            return TestResult.CreateSuccess($"Number {number} is a success!");
        }

        [RuntimeTest]
        public async Task<TestResult> SomeFancyOtherTestAsync(bool someBooleanParameter = true)
        {
            await Task.Delay(100);
            return TestResult.CreateSuccess($"Boolean value is {someBooleanParameter}!");
        }

        [RuntimeTest(
            Name = "Test of warning result with a data dump",
            Description = "This test returns a warning no matter what the input is."
        )]
        public async Task<TestResult> ThisOneShouldReturnAWarning(int id = 123, string key = "key_asd", int days = 7, bool reverse = false)
        {
            await Task.Delay(400);

            var warningText = string.Join(" ", Enumerable.Range(1, 24).Select(x => $"Some more warnings here."));
            var result = TestResult.CreateWarning($"This is a warning! Input was ({id}, {key}, {days}, {reverse}). {warningText}");
            result.AddSerializedData(result, new NewtonsoftJsonSerializer());
            result.AddData("Some extra detail item here. Etc etc.");
            result.AddData("Has something: true\nHasSomething else: false", "Property with id: 12456426");

            return result;
        }

        [RuntimeTest]
        public async Task<TestResult> ThisOneShouldReturnAnError()
        {
            await Task.Delay(800);
            return TestResult.CreateError($"This is an error!");
        }
    }
}
