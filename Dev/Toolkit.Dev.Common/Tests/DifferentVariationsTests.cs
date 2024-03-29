using QoDL.Toolkit.Core.Extensions;
using QoDL.Toolkit.Core.Models;
using QoDL.Toolkit.Core.Modules.Tests.Attributes;
using QoDL.Toolkit.Core.Modules.Tests.Models;
using QoDL.Toolkit.Core.Modules.Tests.Utils.HtmlPresets;
using QoDL.Toolkit.WebUI.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static QoDL.Toolkit.Dev.Common.Jobs.DummyJobsSource;

namespace QoDL.Toolkit.Dev.Common.Tests;

[RuntimeTestClass(
    Name = "Different variations",
    DefaultRolesWithAccess = RuntimeTestAccessRole.WebAdmins,
    GroupName = RuntimeTestConstants.Group.TopGroup,
    UIOrder = 5000,
    AllowRunAll = true
)]
public class DifferentVariationsTests
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
    [RuntimeTestParameter(target: "textArea", "Text Area", "Testing a text area here", TKUIHint.TextArea | TKUIHint.FullWidth)]
    [RuntimeTestParameter(target: "codeArea", "Code Area", "Testing a code area here", TKUIHint.CodeArea)]
    [RuntimeTestParameter(target: "sqlCodeArea", "SQL ode Area", "", TKUIHint.CodeArea, CodeLanguage = "sql")]
    [RuntimeTestParameter(target: "nullableEnm", "Nullable Enum", "Some description", nullName: "<any>")]
    [RuntimeTestParameter(target: "textPattern1", "String with pattern A", "Testing some pattern validation.", TextPattern = @"^O\-\d+")]
    [RuntimeTestParameter(target: "textPattern2", "String with pattern B", "Testing some pattern validation.", TextPattern = @"/^X\-\d+/gi")]
    public TestResult TestParameterTypes(
        (string, int, bool?) tuples,
        Guid guid, Guid? nullableGuid,
        [RuntimeTestParameter(UIHints = TKUIHint.AllowRandom)] Guid guidWithRng,
        [RuntimeTestParameter(UIHints = TKUIHint.AllowRandom)] Guid? nullableGuidWithRng,
        DateTime date, [RuntimeTestParameter(nullName: "<no date>")] DateTime? nullableDate = null,
        DateTimeOffset dateOffset = default, [RuntimeTestParameter(nullName: "<no datetimeoffset>")] DateTimeOffset? nullableDateOffset = null,
        [RuntimeTestParameter(UIHints = TKUIHint.DateRange)] DateTime[] dateRange = default,
        [RuntimeTestParameter(UIHints = TKUIHint.DateRange)] DateTimeOffset[] dateOffsetRange = default,
        [RuntimeTestParameter(UIHints = TKUIHint.DateRange)] DateTime?[] nullableDateRange = default,
        [RuntimeTestParameter(UIHints = TKUIHint.DateRange)] DateTimeOffset?[] nullableDateOffsetRange = default,
        string textPattern1 = "O-1234",
        string textPattern2 = "X1234",
        string text = "abc",
        string textArea = "abc\ndef",
        int number = 123, [RuntimeTestParameter(nullName: "<no number pls>")] int? nullableNumber = 321,
        long largeNumber = 214124112412123L, [RuntimeTestParameter(nullName: "No long thank you")] long? nullableLargeNumber = 214124112412123L,
        bool boolean = true, [RuntimeTestParameter(nullName: "Maybe?")] bool? nullableBool = null,
        float flooot = 12.34f, [RuntimeTestParameter(nullName: "FLOAT!")] float? nullableFlooot = null,
        decimal dec = 11.22m, [RuntimeTestParameter(nullName: "default")] decimal? nullableDec = null,
        double dbl = 22.33, [RuntimeTestParameter(nullName: "(nope)")] double? nullableDbl = null,
        EnumTestType enm = EnumTestType.SecondValue,
        [RuntimeTestParameter(nullName: "What's this?")] EnumTestType? nullableEnm = EnumTestType.ThirdValue,
        [RuntimeTestParameter(nullName: "No null pls")] EnumTestType? nullableEnmDefNull = null,
        EnumFlagsTestType enumFlags = EnumFlagsTestType.A | EnumFlagsTestType.B | EnumFlagsTestType.C,
        byte[] byteArray = null, List<byte[]> listOfByteArray = null,
        string codeArea = "{ \"a\": true }",
        string sqlCodeArea = "SELECT * FROM user_users U WHERE U.Name like '%asd%'"
    )
    {
        return TestResult
            .CreateSuccess($"Ok - |" +
            $"dateRange:{Dump(dateRange)}|dateOffsetRange:{Dump(dateOffsetRange)}|" +
            $"nullableDateRange:{Dump(nullableDateRange)}|nullableDateOffsetRange:{Dump(nullableDateOffsetRange)}|" +
            $"guid:{guid}, nullableGuid:{nullableGuid}, nullableBool:{(nullableBool?.ToString() ?? "null")}|" +
            $"enm:{enm}|nullableEnm:{nullableEnm}|nullableEnmDefNull:{nullableEnmDefNull}|Code:{codeArea}|" +
            $"date:{date}|nullableDate:{nullableDate}|dateOffset:{dateOffset}|" +
            $"nullableDateOffset:{nullableDateOffset}|" +
            $"tuples.Item1:{tuples.Item1}|" +
            $"tuples.Item2:{tuples.Item2}|" +
            $"tuples.Item3:{tuples.Item3}|")
            .SetParametersFeedback(x => text == "all" ? "Some error here" : null)
            .SetParameterFeedback(nameof(text), "Should not be empty pls", () => string.IsNullOrWhiteSpace(text));
    }

    private static string Dump(DateTime?[] dates)
    {
        var a = dates?.Length > 0 ? dates[0].ToString() ?? "null" : "null";
        var b = dates?.Length > 1 ? dates[1].ToString() ?? "null" : "null";
        return $"{a} => {b}";
    }

    private static string Dump(DateTime[] dates)
    {
        var a = dates?.Length > 0 ? dates[0].ToString() ?? "null" : "null";
        var b = dates?.Length > 1 ? dates[1].ToString() ?? "null" : "null";
        return $"{a} => {b}";
    }

    private static string Dump(DateTimeOffset[] dates)
    {
        var a = dates?.Length > 0 ? dates[0].ToString() ?? "null" : "null";
        var b = dates?.Length > 1 ? dates[1].ToString() ?? "null" : "null";
        return $"{a} => {b}";
    }

    private static string Dump(DateTimeOffset?[] dates)
    {
        var a = dates?.Length > 0 ? dates[0].ToString() ?? "null" : "null";
        var b = dates?.Length > 1 ? dates[1].ToString() ?? "null" : "null";
        return $"{a} => {b}";
    }

    [RuntimeTest]
    public TestResult TestAllDataDumpTypes(int imageWidth = 640, int imageHeight = 480, int imageCount = 10, int linkCount = 4, long largeNumber = 214124112412123L)
    {
        var objectToSerialize = TestResult.CreateWarning($"Some random json object");
        var dataTableItems = Enumerable.Range(0, 8000)
            .Select((x, i) => new DummyItem
            {
                EndedAt = DateTimeOffset.Now.AddMinutes(-i),
                IsEnabled = i % 2 == 0,
                IsRunning = i % 5 == 0,
                JobId = $"{typeof(DummyJob).FullName}_{typeof(DummyJob).Assembly.ShortName()}",
                SourceId = Guid.NewGuid().ToString(),
                Summary = $"Summary here {Guid.NewGuid()}.",
                StartedAt = DateTimeOffset.Now.AddDays(-1).AddHours(-5).AddMinutes(i),
                LastRunWasSuccessful = i % 3 == 0,
                NextExecutionScheduledAt = DateTimeOffset.Now.AddMinutes(5 + (i))
            })
            .ToArray();

        return TestResult.CreateSuccess($"Data has been served - {largeNumber}")
            .AddDiff(
                original: "{\n\t\"value\": true\n}",
                modified: "{\n\t\"value\": false\n\t\"Another\": \"nope\"\n}",
                title: "Test A vs Test B"
            )
            .AddDiff(
                original: new { Value = true },
                modified: new { Value = false, Another = "nope" },
                title: "Test serialized A vs Test serialized B"
            )
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
            .AddJsonData("{ \"test\": true }", "Json data")
            .AddUrlsData(Enumerable.Range(1, linkCount).Select(x => new HyperLink($"Link number #{x}", $"{"https://"}www.google.com?q={x}")))
            .AddImageUrlsData(Enumerable.Range(1, imageCount).Select(x => $"{"https://"}loremflickr.com/{imageWidth}/{imageHeight}?v={x}"), $"{imageCount} images from https://loremflickr.com")
            .AddDataTable(dataTableItems)
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
    [RuntimeTestParameter("value", "Value", "Some description here.", DefaultValueFactoryMethod = nameof(SimpleString_Default))]
    public TestResult DefaultValueFactoryMethodWithoutParameterTest(string value)
    {
        return TestResult.CreateSuccess($"Value: {value}");
    }
    public static string SimpleString_Default() => $"The default value. @ {DateTime.Now}";

    [RuntimeTest]
    [RuntimeTestParameter("value", "Value", "Some description here.", DefaultValueFactoryMethod = nameof(SimpleStringCached_Default), UIHints = TKUIHint.CodeArea)]
    public TestResult DefaultValueFactoryMethodWithCacheTest(string value)
    {
        return TestResult.CreateSuccess($"Value: {value}");
    }
    public static TKDefaultTestParameterValue<string> SimpleStringCached_Default() => new($"Value created @ {DateTime.Now}\n😎", TimeSpan.FromSeconds(10));

    [RuntimeTest]
    [RuntimeTestParameter("value", "Value", "Some description here.", DefaultValueFactoryMethod = nameof(AdvancedString_Default))]
    [RuntimeTestParameter("anotherValue", "Another Value", "Some description here.", DefaultValueFactoryMethod = nameof(AdvancedString_Default))]
    public TestResult DefaultValueFactoryMethodWithParameterTest(string value, string anotherValue)
    {
        return TestResult.CreateSuccess($"Value: {value} and {anotherValue}");
    }
    public static string AdvancedString_Default(string parameterName) => $"Name is '{parameterName}'.";


    [RuntimeTest]
    public TestResult TestCleanMode()
    {
        return TestResult.CreateSuccess("This is clean mode.")
            .SetCleanMode()
            .AddCodeData("// Some code here");
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
        uiHints: TKUIHint.ReadOnlyList, DefaultValueFactoryMethod = nameof(ReadOnlyListTest_Default))]
    [RuntimeTestParameter("enumList", "A read only enum list", "Fancy description 2",
        uiHints: TKUIHint.ReadOnlyList, DefaultValueFactoryMethod = nameof(ReadOnlyListEnumTest_Default))]
    public TestResult ReadOnlyListTest(List<string> stringList, List<EnumTestType> enumList)
    {
        return TestResult.CreateSuccess($"Got lists")
            .AddSerializedData(stringList, "stringList")
            .AddSerializedData(enumList, "enumList");
    }

    public static List<string> ReadOnlyListTest_Default()
        => new() { "Item number one", "Item number two", "Item number three" };

    public static List<EnumTestType> ReadOnlyListEnumTest_Default()
        => new() { EnumTestType.FirstValue, EnumTestType.SecondValue, EnumTestType.FourthValue };

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
        TKTestContext.Log("Start of test");
        TKTestContext.StartTiming("Total");

        TKTestContext.StartTiming("First part with some extra text here and a bit more.");
        await Task.Delay(TimeSpan.FromSeconds(0.15f), cancellationToken);
        TKTestContext.WithCurrentResult(x => x.AddTextData("Text added through ModifyCurrentResult"));
        TKTestContext.EndTiming();

        TKTestContext.Log("Middle of test");
        TKTestContext.StartTiming("Last part! Some more details here 🚀");
        await Task.Delay(TimeSpan.FromSeconds(0.35f), cancellationToken);
        TKTestContext.Log("End of test");

        return TestResult.CreateSuccess($"Completed!");
    }

    [RuntimeTest(description: "Should run for about 10 seconds.")]
    public async Task<TestResult> CancellableTest2(CancellationToken cancellationToken)
    {
        for (int i = 0; i < 5; i++)
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

    [RuntimeTest]
    public TestResult AllowAudit(string input)
    {
        return TestResult.CreateSuccess($"AllowAudit output here, input was \"{input}\".");
    }

    [RuntimeTest(HideInputFromAuditLog = true)]
    public TestResult PreventAuditInput(string input)
    {
        return TestResult.CreateSuccess($"PreventAuditInput output here, input was \"{input}\".");
    }

    [RuntimeTest(HideResultMessageFromAuditLog = true)]
    public TestResult PreventAuditResult(string input)
    {
        return TestResult.CreateSuccess($"PreventAuditResult output here, input was \"{input}\".");
    }

    [RuntimeTest(HideResultMessageFromAuditLog = true, HideInputFromAuditLog = true)]
    public TestResult PreventAuditResultAndInput(string input)
    {
        return TestResult.CreateSuccess($"PreventAuditResultAndInput output here, input was \"{input}\".");
    }

    private string PrettifyValue(object value)
    {
        if (value == null) return "null";
        else if (value.GetType().IsValueType) return value.ToString();
        else return $"'{value}'";
    }
}
