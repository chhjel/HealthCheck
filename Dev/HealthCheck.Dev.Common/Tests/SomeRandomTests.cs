using HealthCheck.Core.Modules.SiteEvents.Enums;
using HealthCheck.Core.Modules.SiteEvents.Models;
using HealthCheck.Core.Modules.Tests.Attributes;
using HealthCheck.Core.Modules.Tests.Models;
using HealthCheck.Core.Modules.Tests.Utils.HtmlPresets;
using HealthCheck.WebUI.Extensions;
using HealthCheck.WebUI.Serializers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
#pragma warning disable S3928 // Parameter names used into ArgumentException constructors should match an existing one 

namespace HealthCheck.Dev.Common.Tests
{
#pragma warning disable IDE0060 // Remove unused parameter
    [RuntimeTestClass(
        Name = "Some fancy tests: X",
        Description = "Some fancy <a href=\"https://www.google.com\">description</a>.",
        DefaultRolesWithAccess = RuntimeTestAccessRole.SystemAdmins,
        GroupName = RuntimeTestConstants.Group.AdminStuff,
        UIOrder = 500,
        AllowRunAll = false
    )]
    public class SomeRandomTests
    {
        public enum EnumTestType
        {
            FirstValue = 0,
            SecondValue,
            ThirdValue,
            FourthValue
        }

        [Flags]
        public enum EnumFlagsTestType
        {
            None = 0,
            A = 1,
            B = 2,
            C = 4,
            D = 8
        }

        [RuntimeTest]
        [RuntimeTestParameter(target: "textArea", "Text Area", "Testing a text area here", RuntimeTestParameterAttribute.UIHint.TextArea | RuntimeTestParameterAttribute.UIHint.FullWidth)]
        [RuntimeTestParameter(target: "codeArea", "Code Area", "Testing a code area here", RuntimeTestParameterAttribute.UIHint.CodeArea)]
        public TestResult TestParameterTypes(
            DateTime date, DateTime? nullableDate = null,
            DateTimeOffset dateOffset = default, DateTimeOffset? nullableDateOffset = null,
            string text = "abc",
            string textArea = "abc\ndef",
            int number = 123, int? nullableNumber = 321,
            long largeNumber = 214124112412123L, long? nullableLargeNumber = 214124112412123L,
            bool boolean = true, bool? nullableBool = null,
            float flooot = 12.34f, float? nullableFlooot = null,
            decimal dec = 11.22m, decimal? nullableDec = null,
            double dbl = 22.33, double? nullableDbl = null,
            EnumTestType enm = EnumTestType.SecondValue,
            EnumFlagsTestType enumFlags = EnumFlagsTestType.A | EnumFlagsTestType.B | EnumFlagsTestType.C,
            byte[] byteArray = null, List<byte[]> listOfByteArray = null,
            string codeArea = "{ a: true }"
        )
        {
            return TestResult.CreateSuccess($"Ok - code is: {codeArea}");
        }

        [RuntimeTest]
        public TestResult TestAllDataDumpTypes(int imageWidth = 640, int imageHeight = 480, int imageCount = 10, int linkCount = 4, long largeNumber = 214124112412123L)
        {
            var objectToSerialize = TestResult.CreateWarning($"Some random json object");

            return TestResult.CreateSuccess($"Data has been served - {largeNumber}")
                .AddTimelineData(new[]
                {
                    new TimelineStep("Cart created", "A cart was created")
                        .SetTimestamp(DateTimeOffset.Now.AddMinutes(-15), hideTime: true)
                        .SetIcon("shopping_cart")
                        .SetCompleted(),
                    new TimelineStep("Payment recieved", "The payment was recieved in the system etc", DateTimeOffset.Now.AddMinutes(-7))
                        .AddUrl("https://www.google.com", "Some url here etc")
                        .SetCompleted(),
                    new TimelineStep("Order recieved", "The order was recieved in the system etc")
                        .SetTimestamp(DateTimeOffset.Now.AddMinutes(-3)).SetIcon("receipt").SetError("Oh no! It failed! Some more descriptions here etc etc etc etc and some more!"),
                    new TimelineStep("Order shipped", "Stuff was sent")
                        .AddUrl("https://www.google.com", "Some url here")
                        .SetIcon("local_shipping")
                }, "Timeline test")
                .AddCodeData("public class Test {\n\tpublic string Prop { get; set; }\n}\n", "Code test")
                .AddXmlData("<test>\n\t<el test=\"asd\">Some Value</el>\n</test>\n", "Xml test")
                .AddHtmlData($"Some <b>html</b> here!<br /><a href='https://www.google.com'>some link</a>", "Some html")
                .AddSerializedData(objectToSerialize, "Serialized object data")
                .AddTextData("Some text data", "Text title")
                .AddJsonData(LargeJson, "Big json data")
                .AddUrlsData(Enumerable.Range(1, linkCount).Select(x => new HyperLink($"Link number #{x}", $"{"https://"}www.google.com?q={x}")))
                .AddImageUrlsData(Enumerable.Range(1, imageCount).Select(x => $"{"https://"}loremflickr.com/{imageWidth}/{imageHeight}?v={x}"), $"{imageCount} images from https://loremflickr.com")
                .AddHtmlData(new HtmlPresetBuilder()
                                .AddItems(
                                    new HtmlPresetHeader("Title woop!"),
                                    new HtmlPresetText(@"Some text 
		                            here with <special\';.things>"),
                                    new HtmlPresetSpace(),
                                    new HtmlPresetIFrame("http://localhost:32350"),
                                    new HtmlPresetProgressbar("200", "100"),
                                    new HtmlPresetProgressbar(0.75f),
                                    new HtmlPresetSpace(),
                                    new HtmlPresetKeyValueList()
                                        .UseTableStyle("Key", "Value here")
                                        .AddItem("Price", "123.224,99")
                                        .AddItem("Location", "Oslo")
                                        .AddItem("Name", "Ulven T"),
                                    new HtmlPresetKeyValueList(encodeData: false)
                                        .UseTableStyle("Html test", "Should be progress below")
                                        .AddItem("Progress", new HtmlPresetProgressbar(0.92f).ToHtml()),
                                    new HtmlPresetRaw("<hr />"),
                                    new HtmlPresetList()
                                        .AddItem("Item A")
                                        .AddItem("Item B")
                                        .AddItem("Item C"),
                                    new HtmlPresetImage("https://www.w3schools.com/w3css/img_lights.jpg")
                                )
                );
        }
        [RuntimeTest]
        public TestResult TestCleanMode()
        {
            return TestResult.CreateSuccess("This is clean mode.")
                .SetCleanMode()
                .AddCodeData("// Some code here");
        }

        [RuntimeTest]
        public TestResult TestTimeline(int orderId)
        {
            return TestResult.CreateSuccess("")
                .SetCleanMode()
                .AddTimelineData(new[]
                {
                    new TimelineStep($"Cart #{orderId} created", "A cart was created")
                        .SetTimestamp(DateTime.Now.AddMinutes(-15), hideTime: true)
                        .SetIcon("shopping_cart")
                        .SetCompleted(),
                    new TimelineStep("Payment recieved", "The payment was recieved in the system etc", DateTimeOffset.Now.AddMinutes(-7))
                        .AddUrl("https://www.google.com", "Some url here etc")
                        .SetCompleted(),
                    new TimelineStep("Order recieved", "The order was recieved in the system etc")
                        .SetTimestamp(DateTimeOffset.Now.AddMinutes(-3)).SetIcon("receipt").SetError("Oh no! It failed! Some more descriptions here etc etc etc etc and some more!"),
                    new TimelineStep("Order shipped", "Stuff was sent")
                        .AddUrls(new [] {
                            new HyperLink("Google", "https://www.google.com"),
                            new HyperLink("VG", "https://www.vg.com")
                        })
                        .SetIcon("local_shipping")
                });
        }

        [RuntimeTest(
            Description = "Some <a href=\"https://www.google.com\">description</a> here.",
            RunButtonText = "Import", RunningButtonText = "Importing")]
        [RuntimeTestParameter(Target = "number", Description = "Some <b>fancy</b> text! :D <a href=\"https://www.google.com\">woop</a>")]
        public async Task<TestResult> TestWithoutDefaultValues(int number, string text, bool toggle, DateTime date, DateTimeOffset dateOffset,
            EnumTestType enumParam, EnumFlagsTestType enumFlagsParam,
            List<string> stringList, List<DateTime> dateList, List<DateTime> dateOffsetList, List<bool> boolList, List<EnumTestType> enumList)
        {
            await Task.Delay(TimeSpan.FromSeconds(2));
            var result = TestResult.CreateSuccess($"Recieved: [{PrettifyValue(number)}, {PrettifyValue(text)}, " +
                $"{PrettifyValue(toggle)}, {PrettifyValue(date)}, {PrettifyValue(dateOffset)}, {PrettifyValue(enumParam)}, {PrettifyValue(enumFlagsParam)}]")
                .AddSerializedData(stringList, "stringList")
                .AddSerializedData(dateList, "dateList")
                .AddSerializedData(dateOffsetList, "dateList")
                .AddSerializedData(boolList, "boolList")
                .AddSerializedData(enumList, "enumList");
            return await Task.FromResult(result);
        }

        [RuntimeTest]
        public TestResult TestWithDefaultValues(int number = 123, string text = "some text", bool toggle = true,
            EnumTestType enumParam = EnumTestType.SecondValue, 
            EnumFlagsTestType enumFlagsParam = EnumFlagsTestType.B | EnumFlagsTestType.D)
        //List<string> stringList, List<DateTime> dateList, List<bool> boolList, List<EnumTestType> enumList
        {
            return TestResult.CreateSuccess($"Recieved: [{PrettifyValue(number)}, {PrettifyValue(text)}, " +
                $"{PrettifyValue(toggle)}, {PrettifyValue(enumParam)}, {PrettifyValue(enumFlagsParam)}]");
        }

        [RuntimeTest]
        [RuntimeTestParameter("stringList", "A read only string list", "Fancy description 1", 
            uiHints: RuntimeTestParameterAttribute.UIHint.ReadOnlyList, DefaultValueFactoryMethod = nameof(ReadOnlyListTest_Default))]
        [RuntimeTestParameter("enumList", "A read only enum list", "Fancy description 2",
            uiHints: RuntimeTestParameterAttribute.UIHint.ReadOnlyList, DefaultValueFactoryMethod = nameof(ReadOnlyListEnumTest_Default))]
        public TestResult ReadOnlyListTest(List<string> stringList, List<EnumTestType> enumList)
        {
            return TestResult.CreateSuccess($"Got lists")
                .AddSerializedData(stringList, "stringList")
                .AddSerializedData(enumList, "enumList");
        }

        public static List<string> ReadOnlyListTest_Default()
            => new List<string>() { "Item number one", "Item number two", "Item number three" };

        public static List<EnumTestType> ReadOnlyListEnumTest_Default()
            => new List<EnumTestType>() { EnumTestType.FirstValue, EnumTestType.SecondValue, EnumTestType.FourthValue };

        [RuntimeTest(description: "Should run for about 10 seconds.")]
        public async Task<TestResult> CancellableTest(CancellationToken cancellationToken)
        {
            await Task.Delay(TimeSpan.FromSeconds(10), cancellationToken);
            await Task.Delay(TimeSpan.FromSeconds(2));
            return TestResult.CreateSuccess("Completed!");
        }

        [RuntimeTest(description: "Should run for about 0.5 seconds.")]
        public async Task<TestResult> ContextTest(CancellationToken cancellationToken)
        {
            HCTestContext.Log("Start of test");
            HCTestContext.StartTiming("Total");

            HCTestContext.StartTiming("First part with some extra text here and a bit more.");
            await Task.Delay(TimeSpan.FromSeconds(0.15f), cancellationToken);
            HCTestContext.EndTiming();

            HCTestContext.Log("Middle of test");
            HCTestContext.StartTiming("Last part! Some more details here 🚀");
            await Task.Delay(TimeSpan.FromSeconds(0.35f), cancellationToken);
            HCTestContext.Log("End of test");

            return TestResult.CreateSuccess($"Completed!");
        }

        [RuntimeTest(description: "Should run for about 10 seconds.")]
        public async Task<TestResult> CancellableTest2(CancellationToken cancellationToken)
        {
            for(int i = 0; i < 5; i++)
            {
                await Task.Delay(TimeSpan.FromSeconds(2));
                if (cancellationToken.IsCancellationRequested)
                    break;
            }
            return TestResult.CreateSuccess("Completed!");
        }

        [RuntimeTest(description: "Should run for about 10 seconds.")]
        public async Task<TestResult> CancellableTest3(CancellationToken cancellationToken, int value)
        {
            for (int i = 0; i < 5; i++)
            {
                await Task.Delay(TimeSpan.FromSeconds(2));
                if (cancellationToken.IsCancellationRequested)
                    break;
            }
            return TestResult.CreateSuccess($"Completed! Value was {value}");
        }

        [RuntimeTest]
        public TestResult TestWithNullableValues(int? number = null, bool? checkbox = null, DateTime? date = null, DateTimeOffset? dateOffset = null)
        {
            return TestResult.CreateSuccess($"Recieved: [{PrettifyValue(number)}, {PrettifyValue(checkbox)}, {PrettifyValue(date)}, {PrettifyValue(dateOffset)}]");
        }

        private string PrettifyValue(object value)
        {
            if (value == null) return "null";
            else if (value.GetType().IsValueType) return value.ToString();
            else return $"'{value}'";
        }


        [RuntimeTest(
            Name = "Get data from X",
            Description = "Retrieve some data from some service."
        )]
        [RuntimeTestParameter(target: "id", name: "Data id", description: "Id of the thing to get")]
        [RuntimeTestParameter(target: "orgName", name: "Organization name", description: "Name of the organization the data belongs to",
            uiHints: RuntimeTestParameterAttribute.UIHint.NotNull)]
        [RuntimeTestParameter(target: "latestOnly", name: "Only get the latest data", description: "If true only the latest data will be retrieved")]
        public async Task<TestResult> GetDataFromX(int id = 123, string orgName = "Test Organization", bool latestOnly = false, int someNumber = 42)
        {
            await Task.Delay(300);
            return TestResult.CreateSuccess($"Recieved [{id}, {orgName}, {latestOnly}, {someNumber}] and it was a success!");
        }

        [RuntimeTest("Html result test")]
        public TestResult SomeFancyTest3(int hNumber = 3, string text = "Title")
        {
            return TestResult.CreateSuccess($"Success")
                .AddHtmlData($"This is the h-tag you ordered: <h{hNumber}>{text}</h{hNumber}>");
        }

        [RuntimeTest(Description = "Throws some stuff depending on other stuff not really just throws stuff.")]
        public async Task<TestResult> TestThatThrowsException()
        {
            await Task.Delay(100);
            throw new ArgumentOutOfRangeException("nonExistentArgument");
        }

        [RuntimeTest()]
        public TestResult TestThatIncludesExceptionData()
        {
            try
            {
                int.Parse("something unparsable");
                throw new NotImplementedException("Implemented but broken on purpose!");
            }
            catch (Exception ex)
            {
                return TestResult.CreateError(ex?.Message)
                    .AddExceptionData(ex);
            }
        }

        [RuntimeTest(Category = RuntimeTestConstants.Categories.ScheduledHealthCheck, AllowManualExecution = false)]
        public TestResult TestOfAHealthCheckWarning()
        {
            try
            {
                int.Parse("something unparsable");
                throw new NotImplementedException();
            } catch(Exception ex)
            {
                return TestResult.CreateWarning("Some warning here")
                    .SetSiteEvent(new SiteEvent(SiteEventSeverity.Warning,
                    "IntegrationXLatency", "Increased latency with X", "Integration with X seems to be a bit slower than usual.",
                    duration: 1, developerDetails: $"Exception at {DateTimeOffset.Now}:\n{ex}"));
            }
        }

        [RuntimeTest(Category = RuntimeTestConstants.Categories.ScheduledHealthCheck, AllowManualExecution = false)]
        public TestResult RandomlyResolvesIntegrationXLatency()
        {
            var random = new Random();
            if (random.Next(100) > 80)
            {
                return TestResult.CreateResolvedSiteEvent("OK", "IntegrationXLatency", "Problem now resolved!");
            } else
            {
                return TestResult.CreateWarning("Not resolved yet");
            }
        }

        [RuntimeTest(Category = RuntimeTestConstants.Categories.ScheduledHealthCheck)]
        public TestResult TestOfAHealthCheckError(bool someBool = true, string someStringParam = "test string")
        {
            return TestResult.CreateError("Some error")
                .SetSiteEvent(
                    new SiteEvent(SiteEventSeverity.Error, "IntegrationDataImport",
                        $"Integration Y instabilities ({someStringParam}, {someBool})",
                        "Failed to retrieve data from integration Y, the service might be temporary down.", duration: 5)
                    .AddRelatedLink("Endpoint", "https://www.google.com/?q=some+endpoint+instead+of+this")
                );
        }

        [RuntimeTest(Category = RuntimeTestConstants.Categories.ScheduledHealthCheck)]
        public TestResult TestOfAHealthCheckError(bool? someBool = true, string someStringParam = null, int? number = 222, int? number2 = null)
        {
            return TestResult.CreateError("Some error")
                .SetSiteEvent(
                    new SiteEvent(SiteEventSeverity.Information, "IntegrationZDataImport",
                        $"Integration Z instabilities ({someStringParam}, {someBool}, {number}, {number2})",
                        "Failed to retrieve data from integration Z, the service might be temporary down.", duration: 5)
                    .AddRelatedLink("Endpoint", "https://www.google.com/?q=some+endpoint+instead+of+this")
                    .AddRelatedLink("Page", "https://www.google.com/?q=page")
                );
        }


        [RuntimeTest]
        public async Task<TestResult> TestThatReturnsErrorWithExceptionStackTrace()
        {
            try
            {
                await Task.Delay(100);
                throw new ArgumentOutOfRangeException("nonExistentArgument");
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
        public TestResult SomeFancyTest([RuntimeTestParameter("Id number", "The number on this fancy parameter")] int number = 123)
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
        public async Task<TestResult> ThisOneShouldReturnAWarning([RuntimeTestParameter(description: "Some help text here")] int id = 123, string key = "key_asd", int days = 7, bool reverse = false)
        {
            await Task.Delay(400);

            var warningText = string.Join(" ", Enumerable.Range(1, 24).Select(x => $"Some more warnings here."));
            var result = TestResult.CreateWarning($"This is a warning! Input was ({id}, {key}, {days}, {reverse}). {warningText}");
            result.AddSerializedData(result, new NewtonsoftJsonSerializer());
            result.AddData("Some extra detail item here. Etc etc.");
            result.AddData("Has something: true\nHasSomething else: false", "Property with id: 12456426");

            return result;
        }

        [RuntimeTest("Test: Should return error")]
        public async Task<TestResult> ThisOneShouldReturnAnError()
        {
            await Task.Delay(800);
            return TestResult.CreateError($"This is an error!");
        }

        private const string LargeJson = "[\n"
            + "  {\n"
            + "    \"_id\": \"5d2264f60e07a6e91fdc6931\",\n"
            + "    \"index\": 0,\n"
            + "    \"guid\": \"2f7fcc59-059f-4d65-9949-4d30f4959034\",\n"
            + "    \"isActive\": true,\n"
            + "    \"balance\": \"$2,470.87\",\n"
            + "    \"picture\": \"http://placehold.it/32x32\",\n"
            + "    \"age\": 39,\n"
            + "    \"eyeColor\": \"green\",\n"
            + "    \"name\": {\n"
            + "      \"first\": \"Lola\",\n"
            + "      \"last\": \"Frederick\"\n"
            + "    },\n"
            + "    \"company\": \"JAMNATION\",\n"
            + "    \"email\": \"lola.frederick@jamnation.biz\",\n"
            + "    \"phone\": \"+1 (877) 559-3737\",\n"
            + "    \"address\": \"399 Ryder Avenue, Kula, West Virginia, 8068\",\n"
            + "    \"about\": \"Sint ex tempor laborum dolor commodo sunt sunt nulla minim. Aliqua dolore officia in culpa. Lorem pariatur qui ut magna laborum sunt. Nostrud sit incididunt anim ex tempor aute adipisicing sunt. Nostrud sit commodo adipisicing sit incididunt fugiat consequat. Qui consectetur labore dolor quis commodo laborum quis exercitation. Magna elit dolor incididunt dolor Lorem pariatur dolore et exercitation ea.\",\n"
            + "    \"registered\": \"Wednesday, June 25, 2014 11:23 PM\",\n"
            + "    \"latitude\": \"1.123636\",\n"
            + "    \"longitude\": \"-109.599214\",\n"
            + "    \"tags\": [\n"
            + "      \"fugiat\",\n"
            + "      \"culpa\",\n"
            + "      \"consectetur\",\n"
            + "      \"nostrud\",\n"
            + "      \"ex\"\n"
            + "    ],\n"
            + "    \"range\": [\n"
            + "      0,\n"
            + "      1,\n"
            + "      2,\n"
            + "      3,\n"
            + "      4,\n"
            + "      5,\n"
            + "      6,\n"
            + "      7,\n"
            + "      8,\n"
            + "      9\n"
            + "    ],\n"
            + "    \"friends\": [\n"
            + "      {\n"
            + "        \"id\": 0,\n"
            + "        \"name\": \"Paulette Campbell\"\n"
            + "      },\n"
            + "      {\n"
            + "        \"id\": 1,\n"
            + "        \"name\": \"Hallie Alston\"\n"
            + "      },\n"
            + "      {\n"
            + "        \"id\": 2,\n"
            + "        \"name\": \"Morrison Witt\"\n"
            + "      }\n"
            + "    ],\n"
            + "    \"greeting\": \"Hello, Lola! You have 10 unread messages.\",\n"
            + "    \"favoriteFruit\": \"strawberry\"\n"
            + "  },\n"
            + "  {\n"
            + "    \"_id\": \"5d2264f619151c85023ca9d5\",\n"
            + "    \"index\": 1,\n"
            + "    \"guid\": \"060f02a4-0502-43f1-85ec-9f8139bb3268\",\n"
            + "    \"isActive\": true,\n"
            + "    \"balance\": \"$3,179.95\",\n"
            + "    \"picture\": \"http://placehold.it/32x32\",\n"
            + "    \"age\": 30,\n"
            + "    \"eyeColor\": \"brown\",\n"
            + "    \"name\": {\n"
            + "      \"first\": \"Sparks\",\n"
            + "      \"last\": \"Snider\"\n"
            + "    },\n"
            + "    \"company\": \"ISOPOP\",\n"
            + "    \"email\": \"sparks.snider@isopop.co.uk\",\n"
            + "    \"phone\": \"+1 (833) 550-3562\",\n"
            + "    \"address\": \"365 Bedford Place, Fairlee, South Carolina, 8718\",\n"
            + "    \"about\": \"Nulla nisi in eu elit sit commodo. Voluptate reprehenderit labore elit Lorem excepteur deserunt minim irure. Eiusmod non dolor aliquip sunt adipisicing. Consequat aliqua irure cillum irure anim ut amet minim non qui reprehenderit dolore. Excepteur eu mollit id aute amet consectetur do id anim. Nisi aute veniam commodo fugiat.\",\n"
            + "    \"registered\": \"Monday, August 17, 2015 11:43 PM\",\n"
            + "    \"latitude\": \"59.090596\",\n"
            + "    \"longitude\": \"170.26014\",\n"
            + "    \"tags\": [\n"
            + "      \"anim\",\n"
            + "      \"amet\",\n"
            + "      \"labore\",\n"
            + "      \"occaecat\",\n"
            + "      \"deserunt\"\n"
            + "    ],\n"
            + "    \"range\": [\n"
            + "      0,\n"
            + "      1,\n"
            + "      2,\n"
            + "      3,\n"
            + "      4,\n"
            + "      5,\n"
            + "      6,\n"
            + "      7,\n"
            + "      8,\n"
            + "      9\n"
            + "    ],\n"
            + "    \"friends\": [\n"
            + "      {\n"
            + "        \"id\": 0,\n"
            + "        \"name\": \"Odonnell Sawyer\"\n"
            + "      },\n"
            + "      {\n"
            + "        \"id\": 1,\n"
            + "        \"name\": \"Velma Glenn\"\n"
            + "      },\n"
            + "      {\n"
            + "        \"id\": 2,\n"
            + "        \"name\": \"Pennington Andrews\"\n"
            + "      }\n"
            + "    ],\n"
            + "    \"greeting\": \"Hello, Sparks! You have 7 unread messages.\",\n"
            + "    \"favoriteFruit\": \"apple\"\n"
            + "  },\n"
            + "  {\n"
            + "    \"_id\": \"5d2264f699edd745c42cdc33\",\n"
            + "    \"index\": 2,\n"
            + "    \"guid\": \"0296bc47-0264-4059-84ce-ba91f4744fed\",\n"
            + "    \"isActive\": true,\n"
            + "    \"balance\": \"$3,654.94\",\n"
            + "    \"picture\": \"http://placehold.it/32x32\",\n"
            + "    \"age\": 24,\n"
            + "    \"eyeColor\": \"green\",\n"
            + "    \"name\": {\n"
            + "      \"first\": \"Georgina\",\n"
            + "      \"last\": \"Trujillo\"\n"
            + "    },\n"
            + "    \"company\": \"ZIDOX\",\n"
            + "    \"email\": \"georgina.trujillo@zidox.us\",\n"
            + "    \"phone\": \"+1 (874) 439-3481\",\n"
            + "    \"address\": \"852 Railroad Avenue, Hackneyville, North Dakota, 1725\",\n"
            + "    \"about\": \"Quis consectetur et aliqua adipisicing amet dolor est proident aliquip do deserunt adipisicing ullamco laboris. Minim aliquip dolor ea minim cupidatat est ipsum aliqua dolore cupidatat. Non consequat aute cillum ea. Nisi dolor adipisicing occaecat cillum. Ut ad labore ex non ex exercitation sint do irure tempor. Sunt ex occaecat occaecat nostrud aute tempor. Ad fugiat magna qui cillum nostrud labore ut aliqua amet reprehenderit voluptate.\",\n"
            + "    \"registered\": \"Saturday, November 15, 2014 5:48 AM\",\n"
            + "    \"latitude\": \"-26.878266\",\n"
            + "    \"longitude\": \"-73.588183\",\n"
            + "    \"tags\": [\n"
            + "      \"consectetur\",\n"
            + "      \"nulla\",\n"
            + "      \"ut\",\n"
            + "      \"exercitation\",\n"
            + "      \"id\"\n"
            + "    ],\n"
            + "    \"range\": [\n"
            + "      0,\n"
            + "      1,\n"
            + "      2,\n"
            + "      3,\n"
            + "      4,\n"
            + "      5,\n"
            + "      6,\n"
            + "      7,\n"
            + "      8,\n"
            + "      9\n"
            + "    ],\n"
            + "    \"friends\": [\n"
            + "      {\n"
            + "        \"id\": 0,\n"
            + "        \"name\": \"Pugh Townsend\"\n"
            + "      },\n"
            + "      {\n"
            + "        \"id\": 1,\n"
            + "        \"name\": \"Belinda Sharp\"\n"
            + "      },\n"
            + "      {\n"
            + "        \"id\": 2,\n"
            + "        \"name\": \"Giles Guerra\"\n"
            + "      }\n"
            + "    ],\n"
            + "    \"greeting\": \"Hello, Georgina! You have 8 unread messages.\",\n"
            + "    \"favoriteFruit\": \"banana\"\n"
            + "  },\n"
            + "  {\n"
            + "    \"_id\": \"5d2264f66909de14b6562156\",\n"
            + "    \"index\": 3,\n"
            + "    \"guid\": \"39099b1b-0160-4796-8116-456304d92127\",\n"
            + "    \"isActive\": true,\n"
            + "    \"balance\": \"$3,994.04\",\n"
            + "    \"picture\": \"http://placehold.it/32x32\",\n"
            + "    \"age\": 26,\n"
            + "    \"eyeColor\": \"brown\",\n"
            + "    \"name\": {\n"
            + "      \"first\": \"Deleon\",\n"
            + "      \"last\": \"Steele\"\n"
            + "    },\n"
            + "    \"company\": \"VETRON\",\n"
            + "    \"email\": \"deleon.steele@vetron.tv\",\n"
            + "    \"phone\": \"+1 (952) 515-3148\",\n"
            + "    \"address\": \"929 Tapscott Street, Disautel, Oklahoma, 4215\",\n"
            + "    \"about\": \"Officia in ex reprehenderit occaecat pariatur eiusmod nisi officia sint nulla dolor ea ex commodo. Exercitation ad veniam laboris labore Lorem mollit velit non exercitation deserunt duis anim aliqua. Laborum incididunt nostrud tempor nisi voluptate nostrud eiusmod pariatur qui mollit eiusmod sunt enim. In elit adipisicing magna adipisicing sint velit. Cupidatat aliqua occaecat non dolore. Mollit cillum laborum in laborum reprehenderit ut. Amet aliquip magna sint proident do.\",\n"
            + "    \"registered\": \"Saturday, February 13, 2016 11:50 AM\",\n"
            + "    \"latitude\": \"-60.173622\",\n"
            + "    \"longitude\": \"-178.321036\",\n"
            + "    \"tags\": [\n"
            + "      \"irure\",\n"
            + "      \"fugiat\",\n"
            + "      \"nisi\",\n"
            + "      \"ut\",\n"
            + "      \"amet\"\n"
            + "    ],\n"
            + "    \"range\": [\n"
            + "      0,\n"
            + "      1,\n"
            + "      2,\n"
            + "      3,\n"
            + "      4,\n"
            + "      5,\n"
            + "      6,\n"
            + "      7,\n"
            + "      8,\n"
            + "      9\n"
            + "    ],\n"
            + "    \"friends\": [\n"
            + "      {\n"
            + "        \"id\": 0,\n"
            + "        \"name\": \"Ware Pollard\"\n"
            + "      },\n"
            + "      {\n"
            + "        \"id\": 1,\n"
            + "        \"name\": \"Murray Harris\"\n"
            + "      },\n"
            + "      {\n"
            + "        \"id\": 2,\n"
            + "        \"name\": \"Deirdre Rush\"\n"
            + "      }\n"
            + "    ],\n"
            + "    \"greeting\": \"Hello, Deleon! You have 7 unread messages.\",\n"
            + "    \"favoriteFruit\": \"apple\"\n"
            + "  },\n"
            + "  {\n"
            + "    \"_id\": \"5d2264f6e46648b9e0b51c5f\",\n"
            + "    \"index\": 4,\n"
            + "    \"guid\": \"43b7243f-83d8-4cf2-8147-040e2f1749a9\",\n"
            + "    \"isActive\": true,\n"
            + "    \"balance\": \"$1,865.38\",\n"
            + "    \"picture\": \"http://placehold.it/32x32\",\n"
            + "    \"age\": 23,\n"
            + "    \"eyeColor\": \"green\",\n"
            + "    \"name\": {\n"
            + "      \"first\": \"Rae\",\n"
            + "      \"last\": \"Carey\"\n"
            + "    },\n"
            + "    \"company\": \"FROSNEX\",\n"
            + "    \"email\": \"rae.carey@frosnex.org\",\n"
            + "    \"phone\": \"+1 (869) 458-3160\",\n"
            + "    \"address\": \"262 Gatling Place, Manila, Vermont, 6273\",\n"
            + "    \"about\": \"Reprehenderit sunt consectetur labore do eu in. Duis ipsum cupidatat minim dolore incididunt ullamco duis elit officia. Eiusmod ea excepteur excepteur non magna ad velit ea. Quis id tempor sint enim mollit incididunt qui ex minim deserunt ullamco adipisicing.\",\n"
            + "    \"registered\": \"Tuesday, June 30, 2015 10:11 PM\",\n"
            + "    \"latitude\": \"-22.679271\",\n"
            + "    \"longitude\": \"-105.052508\",\n"
            + "    \"tags\": [\n"
            + "      \"non\",\n"
            + "      \"qui\",\n"
            + "      \"ipsum\",\n"
            + "      \"id\",\n"
            + "      \"do\"\n"
            + "    ],\n"
            + "    \"range\": [\n"
            + "      0,\n"
            + "      1,\n"
            + "      2,\n"
            + "      3,\n"
            + "      4,\n"
            + "      5,\n"
            + "      6,\n"
            + "      7,\n"
            + "      8,\n"
            + "      9\n"
            + "    ],\n"
            + "    \"friends\": [\n"
            + "      {\n"
            + "        \"id\": 0,\n"
            + "        \"name\": \"Walton Zamora\"\n"
            + "      },\n"
            + "      {\n"
            + "        \"id\": 1,\n"
            + "        \"name\": \"Wilma Marsh\"\n"
            + "      },\n"
            + "      {\n"
            + "        \"id\": 2,\n"
            + "        \"name\": \"Harrell Sherman\"\n"
            + "      }\n"
            + "    ],\n"
            + "    \"greeting\": \"Hello, Rae! You have 6 unread messages.\",\n"
            + "    \"favoriteFruit\": \"banana\"\n"
            + "  },\n"
            + "  {\n"
            + "    \"_id\": \"5d2264f6c0d1324aadd1b500\",\n"
            + "    \"index\": 5,\n"
            + "    \"guid\": \"b9e6b78f-e483-4954-a048-f39b256a1c17\",\n"
            + "    \"isActive\": true,\n"
            + "    \"balance\": \"$1,965.40\",\n"
            + "    \"picture\": \"http://placehold.it/32x32\",\n"
            + "    \"age\": 40,\n"
            + "    \"eyeColor\": \"brown\",\n"
            + "    \"name\": {\n"
            + "      \"first\": \"Pickett\",\n"
            + "      \"last\": \"Hunter\"\n"
            + "    },\n"
            + "    \"company\": \"NORSUP\",\n"
            + "    \"email\": \"pickett.hunter@norsup.name\",\n"
            + "    \"phone\": \"+1 (955) 499-3443\",\n"
            + "    \"address\": \"867 Vanderveer Street, Bethpage, New Mexico, 2248\",\n"
            + "    \"about\": \"Et aliqua eiusmod occaecat nostrud est sit sit anim minim occaecat sit ea est ex. Lorem magna irure nostrud reprehenderit sunt. Laborum consectetur magna id aliquip. Exercitation sint minim velit ut cillum sint cupidatat culpa culpa aliquip consectetur deserunt Lorem occaecat. In est nostrud cupidatat non. Sint adipisicing velit esse laborum reprehenderit anim officia deserunt nostrud quis mollit proident. Laboris tempor sit velit officia amet qui veniam.\",\n"
            + "    \"registered\": \"Friday, March 25, 2016 8:14 PM\",\n"
            + "    \"latitude\": \"86.532366\",\n"
            + "    \"longitude\": \"53.390805\",\n"
            + "    \"tags\": [\n"
            + "      \"aliquip\",\n"
            + "      \"in\",\n"
            + "      \"officia\",\n"
            + "      \"ut\",\n"
            + "      \"officia\"\n"
            + "    ],\n"
            + "    \"range\": [\n"
            + "      0,\n"
            + "      1,\n"
            + "      2,\n"
            + "      3,\n"
            + "      4,\n"
            + "      5,\n"
            + "      6,\n"
            + "      7,\n"
            + "      8,\n"
            + "      9\n"
            + "    ],\n"
            + "    \"friends\": [\n"
            + "      {\n"
            + "        \"id\": 0,\n"
            + "        \"name\": \"Carpenter Lloyd\"\n"
            + "      },\n"
            + "      {\n"
            + "        \"id\": 1,\n"
            + "        \"name\": \"Solomon Garrison\"\n"
            + "      },\n"
            + "      {\n"
            + "        \"id\": 2,\n"
            + "        \"name\": \"Lisa Watts\"\n"
            + "      }\n"
            + "    ],\n"
            + "    \"greeting\": \"Hello, Pickett! You have 5 unread messages.\",\n"
            + "    \"favoriteFruit\": \"strawberry\"\n"
            + "  },\n"
            + "  {\n"
            + "    \"_id\": \"5d2264f669709ba8cdd57667\",\n"
            + "    \"index\": 6,\n"
            + "    \"guid\": \"7685353b-6dff-4513-906a-0f33df2ce4e8\",\n"
            + "    \"isActive\": false,\n"
            + "    \"balance\": \"$1,239.73\",\n"
            + "    \"picture\": \"http://placehold.it/32x32\",\n"
            + "    \"age\": 24,\n"
            + "    \"eyeColor\": \"blue\",\n"
            + "    \"name\": {\n"
            + "      \"first\": \"Shauna\",\n"
            + "      \"last\": \"Underwood\"\n"
            + "    },\n"
            + "    \"company\": \"SPACEWAX\",\n"
            + "    \"email\": \"shauna.underwood@spacewax.ca\",\n"
            + "    \"phone\": \"+1 (896) 434-3962\",\n"
            + "    \"address\": \"884 Throop Avenue, Selma, Maryland, 8121\",\n"
            + "    \"about\": \"Ullamco in quis minim et. Duis deserunt reprehenderit duis duis anim dolore mollit magna commodo. Laborum in velit irure eu incididunt mollit ea enim ipsum pariatur irure dolore. Ex exercitation magna quis eu ea esse consectetur ea quis magna non voluptate. Voluptate dolor elit consectetur consectetur consequat adipisicing ullamco est occaecat nostrud deserunt. Et cupidatat deserunt do veniam tempor occaecat labore quis Lorem ea eu.\",\n"
            + "    \"registered\": \"Monday, June 17, 2019 6:20 PM\",\n"
            + "    \"latitude\": \"39.566357\",\n"
            + "    \"longitude\": \"-174.185089\",\n"
            + "    \"tags\": [\n"
            + "      \"aute\",\n"
            + "      \"anim\",\n"
            + "      \"officia\",\n"
            + "      \"anim\",\n"
            + "      \"dolore\"\n"
            + "    ],\n"
            + "    \"range\": [\n"
            + "      0,\n"
            + "      1,\n"
            + "      2,\n"
            + "      3,\n"
            + "      4,\n"
            + "      5,\n"
            + "      6,\n"
            + "      7,\n"
            + "      8,\n"
            + "      9\n"
            + "    ],\n"
            + "    \"friends\": [\n"
            + "      {\n"
            + "        \"id\": 0,\n"
            + "        \"name\": \"Blanchard Gilbert\"\n"
            + "      },\n"
            + "      {\n"
            + "        \"id\": 1,\n"
            + "        \"name\": \"Mae Simon\"\n"
            + "      },\n"
            + "      {\n"
            + "        \"id\": 2,\n"
            + "        \"name\": \"Brittney Sears\"\n"
            + "      }\n"
            + "    ],\n"
            + "    \"greeting\": \"Hello, Shauna! You have 8 unread messages.\",\n"
            + "    \"favoriteFruit\": \"banana\"\n"
            + "  },\n"
            + "  {\n"
            + "    \"_id\": \"5d2264f6d306c0334bc02e7a\",\n"
            + "    \"index\": 7,\n"
            + "    \"guid\": \"f7afaa23-1786-4170-8f67-e731ee3a5421\",\n"
            + "    \"isActive\": false,\n"
            + "    \"balance\": \"$2,130.79\",\n"
            + "    \"picture\": \"http://placehold.it/32x32\",\n"
            + "    \"age\": 25,\n"
            + "    \"eyeColor\": \"green\",\n"
            + "    \"name\": {\n"
            + "      \"first\": \"Bridgette\",\n"
            + "      \"last\": \"Valencia\"\n"
            + "    },\n"
            + "    \"company\": \"NEPTIDE\",\n"
            + "    \"email\": \"bridgette.valencia@neptide.biz\",\n"
            + "    \"phone\": \"+1 (887) 476-3700\",\n"
            + "    \"address\": \"375 Bartlett Place, Finzel, Alaska, 6170\",\n"
            + "    \"about\": \"Laboris velit do culpa reprehenderit duis incididunt nisi proident. Incididunt non velit mollit do ea deserunt ea pariatur ex aliqua. Dolore sit occaecat duis laborum fugiat commodo et minim incididunt amet. Sint amet laboris magna minim. Sit voluptate sint sunt elit deserunt exercitation cillum mollit laboris aliquip. Anim enim ullamco officia est.\",\n"
            + "    \"registered\": \"Friday, March 1, 2019 5:38 AM\",\n"
            + "    \"latitude\": \"-4.849929\",\n"
            + "    \"longitude\": \"-49.427353\",\n"
            + "    \"tags\": [\n"
            + "      \"officia\",\n"
            + "      \"mollit\",\n"
            + "      \"excepteur\",\n"
            + "      \"nostrud\",\n"
            + "      \"dolore\"\n"
            + "    ],\n"
            + "    \"range\": [\n"
            + "      0,\n"
            + "      1,\n"
            + "      2,\n"
            + "      3,\n"
            + "      4,\n"
            + "      5,\n"
            + "      6,\n"
            + "      7,\n"
            + "      8,\n"
            + "      9\n"
            + "    ],\n"
            + "    \"friends\": [\n"
            + "      {\n"
            + "        \"id\": 0,\n"
            + "        \"name\": \"Holmes Daniel\"\n"
            + "      },\n"
            + "      {\n"
            + "        \"id\": 1,\n"
            + "        \"name\": \"Christian Page\"\n"
            + "      },\n"
            + "      {\n"
            + "        \"id\": 2,\n"
            + "        \"name\": \"Liz Rocha\"\n"
            + "      }\n"
            + "    ],\n"
            + "    \"greeting\": \"Hello, Bridgette! You have 6 unread messages.\",\n"
            + "    \"favoriteFruit\": \"strawberry\"\n"
            + "  }\n"
            + "]\n";
    }
#pragma warning restore IDE0060 // Remove unused parameter
}
