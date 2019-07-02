using HealthCheck.Core.Attributes;
using HealthCheck.Core.Entities;
using System.Linq;
using System.Threading.Tasks;

namespace HealthCheck.DevTest._TestImplementation.Tests
{
    [RuntimeTestClass(
        Name = "Some fancy tests",
        Description = "Some fancy description"
    )]
    public class SomeRandomTests
    {
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
            result.AddSerializedData(result);
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
