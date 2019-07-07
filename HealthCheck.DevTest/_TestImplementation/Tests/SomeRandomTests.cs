using HealthCheck.Core.Attributes;
using HealthCheck.Core.Entities;
using HealthCheck.WebUI.Extensions;
using HealthCheck.WebUI.Serializers;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace HealthCheck.DevTest._TestImplementation.Tests
{
    [RuntimeTestClass(
        Name = "Some fancy tests",
        Description = "Some fancy description",
        DefaultRolesWithAccess = RuntimeTestAccessRole.SystemAdmins,
        GroupName = RuntimeTestConstants.Group.AdminStuff,
        Icon = RuntimeTestConstants.Icons.Dashboard,
        UIOrder = 500
    )]
    public class SomeRandomTests
    {
        [RuntimeTest]
        public TestResult TestAllDataDumpTypes(int imageWidth = 640, int imageHeight = 480, int imageCount = 10)
        {
            var objectToSerialize = TestResult.CreateWarning($"Some random json object");

            return TestResult.CreateSuccess($"Images has been served.")
                .AddImageUrlsData(Enumerable.Range(1, imageCount).Select(x => $"{"https://"}picsum.photos/{imageWidth}/{imageHeight}?v={x}"), $"{imageCount} images from https://picsum.photos")
                .AddHtmlData($"Some <b>html</b> here!<br /><a href='https://www.google.com'>some link</a>", "Some html")
                .AddSerializedData(objectToSerialize, "Serialized object data")
                .AddTextData("Some text data", "Text title")
                .AddXmlData("<test><el test=\"asd\">Some Value</el></test>", "Xml test");
        }

        [RuntimeTest(
            Name = "Get data from X",
            Description = "Retrieve some data from some service.",
            ParameterNames = new[] {
                "Data id",
                "Organization name",
                "Only get the latest data"
            },
            ParameterDescriptions = new[] {
                "Id of the thing to get",
                "Name of the organization the data belongs to.",
                "If true only the latest data will be retrieved."
            }
        )]
        public async Task<TestResult> GetDataFromX(int id = 123, string orgName = "Test Organization", bool latestOnly = false, int someNumber = 42)
        {
            await Task.Delay(300);
            return TestResult.CreateSuccess($"Recieved [{id}, {orgName}, {latestOnly}, {someNumber}] and it was a success!");
        }

        [RuntimeTest(
            Name = "Html result test",
            ParameterNames = new[] { "H-number", "Text" },
            ParameterDescriptions = new[] { "What h-tag to return.", "Text in the h-tag" }
        )]
        public TestResult SomeFancyTest(int hNumber = 3, string text = "Title")
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
            Description = "Fancy fancy description",
            ParameterNames = new[] { "Id number" },
            ParameterDescriptions = new[] { "The number on this fancy parameter" }
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
            Description = "This test returns a warning no matter what the input is.",
            ParameterDescriptions = new[] { "Some help text here", "Some help text here", "Some help text here", "Some help text here" }
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
