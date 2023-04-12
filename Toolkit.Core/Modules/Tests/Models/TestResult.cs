using QoDL.Toolkit.Core.Abstractions;
using QoDL.Toolkit.Core.Config;
using QoDL.Toolkit.Core.Modules.SiteEvents.Models;
using QoDL.Toolkit.Core.Modules.Tests.Utils.HtmlPresets;
using QoDL.Toolkit.Core.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QoDL.Toolkit.Core.Modules.Tests.Models;

/// <summary>
/// Result from a test.
/// </summary>
public class TestResult
{
    /// <summary>
    /// Result of the test.
    /// </summary>
    public TestResultStatus Status { get; set; }

    /// <summary>
    /// Test result message.
    /// </summary>
    public string Message { get; set; }

    /// <summary>
    /// Full stack trace of exception if any.
    /// </summary>
    public string StackTrace { get; set; }

    /// <summary>
    /// Expand data in frontend by default.
    /// </summary>
    public bool ExpandDataByDefault { get; set; }

    /// <summary>
    /// Allow expanding the data in frontend. If false the expansion panel header will be hidden.
    /// </summary>
    public bool AllowExpandData { get; set; } = true;

    /// <summary>
    /// Removes expansion panel and copy/fullscreeen buttons.
    /// </summary>
    public bool DisplayClean { get; set; }

    /// <summary>
    /// Feedback per parameters.
    /// </summary>
    public Dictionary<string, string> ParameterFeedback { get; private set; } = new();

    /// <summary>
    /// Feedback per parameter.
    /// </summary>
    public Func<string, string> FeedbackPerParameter { get; set; }

    /// <summary>
    /// The test that was executed.
    /// </summary>
    public TestDefinition Test { get; set; }

    /// <summary>
    /// Any optional data that will be stringified and returned with the result.
    /// </summary>
    public List<TestResultDataDump> Data { get; set; } = new List<TestResultDataDump>();

    /// <summary>
    /// Optional site event.
    /// </summary>
    public SiteEvent SiteEvent { get; set; }

    /// <summary>
    /// Custom data object.
    /// </summary>
    public object Tag { get; set; }

    /// <summary>
    /// How long the test took to execute.
    /// </summary>
    public long DurationInMilliseconds { get; set; }

    /// <summary>
    /// For proxy tests this will be set to the return value from methods.
    /// </summary>
    public object ProxyTestResultObject { get; private set; }

    internal object AutoCreateResultDataFromObject { get; set; }
    internal bool AllowOverrideMessage { get; set; } = false;

    /// <summary>
    /// Create a new test result with the given status.
    /// </summary>
    /// <param name="status">Test status.</param>
    /// <param name="message">Message text. If null exception message will be used if any.</param>
    /// <param name="exception">Exception if any to get stack trace and any other details from.</param>
    public static TestResult Create(TestResultStatus status, string message, Exception exception = null)
        => new()
        {
            Status = status,
            Message = message ?? exception?.Message,
            StackTrace = TKExceptionUtils.GetFullExceptionDetails(exception, returnNullIfNull: true)
        };

    /// <summary>
    /// Create a new test success or error result depending on the given boolean value.
    /// </summary>
    /// <param name="success">True for success, false for failure.</param>
    /// <param name="message">Success or error message. If null exception message will be used if any.</param>
    /// <param name="exception">Exception if any to get stack trace from.</param>
    public static TestResult Create(bool success, string message, Exception exception = null)
        => Create(success ? TestResultStatus.Success : TestResultStatus.Error, message, exception);

    /// <summary>
    /// Create a new successful test result.
    /// </summary>
    /// <param name="message">Message text with some extra details about the success.</param>
    public static TestResult CreateSuccess(string message)
        => Create(TestResultStatus.Success, message);

    /// <summary>
    /// Create a new warning test result.
    /// </summary>
    /// <param name="message">Message text with some extra details about the warning. If null exception message will be used if any.</param>
    /// <param name="exception">Exception if any to get stack trace from.</param>
    public static TestResult CreateWarning(string message, Exception exception = null)
        => Create(TestResultStatus.Warning, message, exception);

    /// <summary>
    /// Create a new failed test result.
    /// </summary>
    /// <param name="message">Error text describing what went wrong. If null exception message will be used if any.</param>
    /// <param name="exception">Exception if any to get stack trace from.</param>
    public static TestResult CreateError(string message, Exception exception = null)
        => Create(TestResultStatus.Error, message, exception);

    /// <summary>
    /// Create a successfull result with a resolved site event, resolving the previous site event with the given <paramref name="eventTypeId"/>.
    /// </summary>
    /// <param name="testResultMessage">Success message in test result.</param>
    /// <param name="eventTypeId">Event type id of previous site event to resolve.</param>
    /// <param name="resolvedMessage">Resolved message.</param>
    public static TestResult CreateResolvedSiteEvent(string testResultMessage, string eventTypeId, string resolvedMessage)
        => Create(TestResultStatus.Success, testResultMessage).SetResolvedSiteEvent(eventTypeId, resolvedMessage);

    #region Method-chaining
    /// <summary>
    /// Sets parameter feedback for a single parameter, for e.g. validation of fields.
    /// </summary>
    public TestResult SetParameterFeedback(string parameterName, string feedback, Func<bool> condition = null)
    {
        if (condition?.Invoke() != false)
        {
            ParameterFeedback[parameterName ?? string.Empty] = feedback;
        }
        return this;
    }

    /// <summary>
    /// Sets parameter feedback conditionally for all parameters, for e.g. validation of fields.
    /// </summary>
    /// <param name="feedbackPerParameter">Input: parameter name. Output: feedback.</param>
    public TestResult SetParametersFeedback(Func<string, string> feedbackPerParameter)
    {
        FeedbackPerParameter = feedbackPerParameter;
        return this;
    }

    /// <summary>
    /// Expand data in frontend by default.
    /// </summary>
    public TestResult SetDataExpandedByDefault(bool expandDataByDefault = true)
    {
        ExpandDataByDefault = expandDataByDefault;
        return this;
    }

    /// <summary>
    /// Disallow expanding the data in frontend, hiding the expansion panel header.
    /// <para>If disallowed the data will be expanded by default.</para>
    /// </summary>
    public TestResult DisallowDataExpansion(bool disallowed = true)
    {
        AllowExpandData = !disallowed;
        return this;
    }

    /// <summary>
    /// Removes expansion panel and copy/fullscreeen buttons.
    /// </summary>
    public TestResult SetCleanMode(bool clean = true)
    {
        DisplayClean = clean;
        return this;
    }

    /// <summary>
    /// Removes expansion panel and copy/fullscreeen buttons for the latest added data.
    /// </summary>
    public TestResult SetLatestDataCleanMode(bool? clean = true)
    {
        var data = Data?.LastOrDefault();
        if (data != null)
        {
            data.DisplayClean = clean;
        }
        return this;
    }

    /// <summary>
    /// Include a serialized version of the given object in the result data.
    /// <para>If using QoDL.Toolkit.WebUI the NewtonsoftJsonSerializer() or just use the AddSerializedData(object data, string title=null) extension method from QoDL.Toolkit.WebUI.</para>
    /// </summary>
    /// <param name="data">Data to serialize as json and display.</param>
    /// <param name="serializer">What serializer implementation to use.</param>
    /// <param name="title">Title of the result data if any.</param>
    /// <param name="onlyIfNotNull">Only include the result if the data is not null or empty.</param>
    /// <param name="downloadFileName">Optionally specify a filename used when downloading the data.</param>
    public TestResult AddSerializedData(object data, IJsonSerializer serializer, string title = null, bool onlyIfNotNull = true, string downloadFileName = null)
    {
        if (data == null && onlyIfNotNull)
            return this;
        else if (serializer == null)
            return this;

        var dump = data.Dump(serializer, title);
        Data.Add(new TestResultDataDump()
        {
            Title = dump.Title,
            Data = dump.Data,
            Type = TestResultDataDumpType.Json,
            DownloadFileName = CreateDownloadFilename(downloadFileName, title, TestResultDataDumpType.Json)
        });
        return this;
    }

    /// <summary>
    /// Include the given content in the result data.
    /// </summary>
    /// <param name="data">Raw content to display.</param>
    /// <param name="title">Title of the result data if any.</param>
    /// <param name="type">How to display the data.</param>
    /// <param name="onlyIfNotNullOrEmpty">Only include the result if the data is not null or empty.</param>
    /// <param name="downloadFileName">Optionally specify a filename, if given a download button will be shown where the html can be downloaded.</param>
    public TestResult AddData(string data, string title = null, TestResultDataDumpType type = TestResultDataDumpType.PlainText, bool onlyIfNotNullOrEmpty = true, string downloadFileName = null)
        => AddData(data, title, type, onlyIfNotNullOrEmpty, downloadFileName, new string[0]);

    /// <summary>
    /// Include details about the given exception in the result data.
    /// <para>Shortcut to AddTextData(ExceptionUtils.GetFullExceptionDetails(exception), ..)</para>
    /// </summary>
    /// <param name="exception">Exception data to display.</param>
    /// <param name="title">Title of the result data if any.</param>
    /// <param name="onlyIfNotNullOrEmpty">Only include the result if the data is not null or empty.</param>
    public TestResult AddExceptionData(Exception exception, string title = null, bool onlyIfNotNullOrEmpty = true)
        => AddTextData(
            (exception != null) ? TKExceptionUtils.GetFullExceptionDetails(exception) : null, 
            title ?? exception?.GetType()?.Name,
            onlyIfNotNullOrEmpty);

    /// <summary>
    /// Include the given plain text in the result data.
    /// </summary>
    /// <param name="text">Content to display.</param>
    /// <param name="title">Title of the result data if any.</param>
    /// <param name="onlyIfNotNullOrEmpty">Only include the result if the data is not null or empty.</param>
    /// <param name="downloadFileName">Optionally specify a filename used when downloading the data.</param>
    public TestResult AddTextData(string text, string title = null, bool onlyIfNotNullOrEmpty = true, string downloadFileName = null)
        => AddData(text, title, TestResultDataDumpType.PlainText, onlyIfNotNullOrEmpty, CreateDownloadFilename(downloadFileName, title, TestResultDataDumpType.PlainText));

    /// <summary>
    /// Include the given code text in the result data. Will be shown in a monaco-editor.
    /// </summary>
    /// <param name="code">Content to display.</param>
    /// <param name="title">Title of the result data if any.</param>
    /// <param name="onlyIfNotNullOrEmpty">Only include the result if the data is not null or empty.</param>
    /// <param name="downloadFileName">Optionally specify a filename used when downloading the data.</param>
    public TestResult AddCodeData(string code, string title = null, bool onlyIfNotNullOrEmpty = true, string downloadFileName = null)
        => AddData(code, title, TestResultDataDumpType.Code, onlyIfNotNullOrEmpty, CreateDownloadFilename(downloadFileName, title, TestResultDataDumpType.Code));

    /// <summary>
    /// Include two values to show in a monaco diff-editor.
    /// </summary>
    /// <param name="original">Original content to diff.</param>
    /// <param name="modified">Modified content to diff.</param>
    /// <param name="title">Title of the result data if any.</param>
    /// <param name="onlyIfNotNullOrEmpty">Only include the result if the data is not null or empty.</param>
    /// <param name="downloadFileName">Optionally specify a filename used when downloading the data.</param>
    public TestResult AddDiff(string original, string modified, string title = null, bool onlyIfNotNullOrEmpty = true, string downloadFileName = null)
    {
        var bakedData = TKGlobalConfig.Serializer.Serialize(new
        {
            Original = original,
            Modified = modified,
        });
        return AddData(bakedData, title, TestResultDataDumpType.Diff, onlyIfNotNullOrEmpty, CreateDownloadFilename(downloadFileName, title, TestResultDataDumpType.Diff));
    }

    /// <summary>
    /// Include two values to show in a monaco diff-editor.
    /// </summary>
    /// <param name="original">Original content to serialize and diff.</param>
    /// <param name="modified">Modified content to serialize and diff.</param>
    /// <param name="title">Title of the result data if any.</param>
    /// <param name="onlyIfNotNullOrEmpty">Only include the result if the data is not null or empty.</param>
    /// <param name="downloadFileName">Optionally specify a filename used when downloading the data.</param>
    public TestResult AddDiff(object original, object modified, string title = null, bool onlyIfNotNullOrEmpty = true, string downloadFileName = null)
    {
        var bakedData = TKGlobalConfig.Serializer.Serialize(new
        {
            Original = TKGlobalConfig.Serializer.Serialize(original),
            Modified = TKGlobalConfig.Serializer.Serialize(modified),
        });
        return AddData(bakedData, title, TestResultDataDumpType.Diff, onlyIfNotNullOrEmpty, CreateDownloadFilename(downloadFileName, title, TestResultDataDumpType.Diff));
    }

    /// <summary>
    /// Include the given html in the result data.
    /// </summary>
    /// <param name="html">Html to render.</param>
    /// <param name="title">Title of the result data if any.</param>
    /// <param name="onlyIfNotNullOrEmpty">Only include the result if the data is not null or empty.</param>
    /// <param name="downloadFileName">Optionally specify a filename, if given a download button will be shown where the html can be downloaded.</param>
    /// <param name="useShadowDom">Wraps the output in a shadow DOM.</param>
    public TestResult AddHtmlData(string html, string title = null, bool onlyIfNotNullOrEmpty = true, string downloadFileName = null, bool useShadowDom = false)
    {
        string[] flags = useShadowDom ? new[] { "ShadowDom" } : null;
        return AddData(html, title, TestResultDataDumpType.Html, onlyIfNotNullOrEmpty, downloadFileName, flags);
    }

    /// <summary>
    /// Include the given xml in the result data.
    /// </summary>
    /// <param name="xml">Content to display.</param>
    /// <param name="title">Title of the result data if any.</param>
    /// <param name="onlyIfNotNullOrEmpty">Only include the result if the data is not null or empty.</param>
    /// <param name="downloadFileName">Optionally specify a filename used when downloading the data.</param>
    public TestResult AddXmlData(string xml, string title = null, bool onlyIfNotNullOrEmpty = true, string downloadFileName = null)
        => AddData(xml, title, TestResultDataDumpType.Xml, onlyIfNotNullOrEmpty, CreateDownloadFilename(downloadFileName, title, TestResultDataDumpType.Xml));

    /// <summary>
    /// Include the given json in the result data.
    /// </summary>
    /// <param name="json">Content to display.</param>
    /// <param name="title">Title of the result data if any.</param>
    /// <param name="onlyIfNotNullOrEmpty">Only include the result if the data is not null or empty.</param>
    /// <param name="downloadFileName">Optionally specify a filename used when downloading the data.</param>
    public TestResult AddJsonData(string json, string title = null, bool onlyIfNotNullOrEmpty = true, string downloadFileName = null)
        => AddData(json, title, TestResultDataDumpType.Json, onlyIfNotNullOrEmpty, CreateDownloadFilename(downloadFileName, title, TestResultDataDumpType.Json));

    /// <summary>
    /// Include the given timing data in the result data.
    /// </summary>
    /// <param name="timing">Timing data to display.</param>
    /// <param name="title">Title of the result data if any.</param>
    /// <param name="onlyIfNotNullOrEmpty">Only include the result if the data is not null or empty.</param>
    public TestResult AddTimingData(TKTestTiming timing, string title = null, bool onlyIfNotNullOrEmpty = true)
        => AddTimingData(new [] { timing }, title, onlyIfNotNullOrEmpty);

    /// <summary>
    /// Include the given timing data in the result data.
    /// </summary>
    /// <param name="timings">Timing data to display.</param>
    /// <param name="title">Title of the result data if any.</param>
    /// <param name="onlyIfNotNullOrEmpty">Only include the result if the data is not null or empty.</param>
    public TestResult AddTimingData(IEnumerable<TKTestTiming> timings, string title = null, bool onlyIfNotNullOrEmpty = true)
        => AddData(TKGlobalConfig.Serializer?.Serialize(timings) ?? "[]", title, TestResultDataDumpType.Timings, onlyIfNotNullOrEmpty);

    /// <summary>
    /// Include the given image urls in the result data.
    /// </summary>
    /// <param name="urls">Image urls to display.</param>
    /// <param name="title">Title of the result data if any.</param>
    /// <param name="onlyIfNotNullOrEmpty">Only include the result if the data is not null or empty.</param>
    public TestResult AddImageUrlsData(IEnumerable<string> urls, string title = null, bool onlyIfNotNullOrEmpty = true)
    {
        urls ??= new string[0];
        if (onlyIfNotNullOrEmpty)
        {
            urls = urls.Where(x => !onlyIfNotNullOrEmpty || !string.IsNullOrWhiteSpace(x));
            if (!urls.Any())
            {
                return this;
            }
        }
        return AddData(string.Join(Environment.NewLine, urls), title, TestResultDataDumpType.ImageUrls, onlyIfNotNullOrEmpty);
    }

    /// <summary>
    /// Include the given image url in the result data.
    /// </summary>
    /// <param name="url">Image urls to display.</param>
    /// <param name="title">Title of the result data if any.</param>
    /// <param name="onlyIfNotNullOrEmpty">Only include the result if the data is not null or empty.</param>
    public TestResult AddImageUrlData(string url, string title = null, bool onlyIfNotNullOrEmpty = true)
        => AddImageUrlsData(new[] { url }, title, onlyIfNotNullOrEmpty);

    /// <summary>
    /// Include the given urls in the result data.
    /// </summary>
    /// <param name="urls">Urls to display.</param>
    /// <param name="title">Title of the result data if any.</param>
    /// <param name="onlyifAnyUrls">Only include the result if the data is not null or empty.</param>
    public TestResult AddUrlsData(IEnumerable<HyperLink> urls, string title = null, bool onlyifAnyUrls = true)
    {
        urls ??= new HyperLink[0];
        if (!urls.Any() && onlyifAnyUrls) return this;
        var stringUrls = urls.Where(x => x != null).Select(x => $"{x.Text.Replace("=>", "->")} => {x.Url}");
        AddData(string.Join(Environment.NewLine, stringUrls), title, TestResultDataDumpType.Urls);
        return this;
    }

    /// <summary>
    /// Include the given urls in the result data.
    /// </summary>
    /// <param name="urls">Urls to display.</param>
    /// <param name="title">Title of the result data if any.</param>
    /// <param name="onlyIfNotNullOrEmpty">Only include the result if the data is not null or empty.</param>
    public TestResult AddUrlsData(IEnumerable<string> urls, string title = null, bool onlyIfNotNullOrEmpty = true)
        => AddUrlsData(urls?.Select(x => new HyperLink(x, x)), title, onlyIfNotNullOrEmpty);

    /// <summary>
    /// Include the given url in the result data.
    /// </summary>
    /// <param name="url">Url to display.</param>
    /// <param name="title">Title of the result data if any.</param>
    /// <param name="onlyIfNotNullOrEmpty">Only include the result if the data is not null or empty.</param>
    public TestResult AddUrlData(string url, string title = null, bool onlyIfNotNullOrEmpty = true)
        => AddUrlData(new HyperLink(url, url), title, onlyIfNotNullOrEmpty);

    /// <summary>
    /// Include the given url in the result data.
    /// </summary>
    /// <param name="link">Url to display.</param>
    /// <param name="title">Title of the result data if any.</param>
    /// <param name="onlyIfNotNullOrEmpty">Only include the result if the data is not null or empty.</param>
    public TestResult AddUrlData(HyperLink link, string title = null, bool onlyIfNotNullOrEmpty = true)
        => AddUrlsData(
            (string.IsNullOrWhiteSpace(link?.Url) && onlyIfNotNullOrEmpty) ? new HyperLink[0] : new[] { link },
            title, onlyIfNotNullOrEmpty);

    /// <summary>
    /// Creates a sortable, filterable datatable from the given items.
    /// <para>Only top level, public properties will be included.</para>
    /// </summary>
    public TestResult AddDataTable(IList<object> items, string title = null, bool onlyIfNotNullOrEmpty = true)
        => AddHtmlData(
            new HtmlPresetBuilder().AddItem(new HtmlPresetDataTable().AddItems(items)).ToHtml(),
            title, onlyIfNotNullOrEmpty
        );

    /// <summary>
    /// Include a timeline built from the given data in the result data.
    /// </summary>
    /// <param name="steps">Timeline steps to render.</param>
    /// <param name="title">Title of the result data if any.</param>
    /// <param name="onlyIfAnySteps">Only include the result if the data is not null or empty.</param>
    public TestResult AddTimelineData(IEnumerable<TimelineStep> steps, string title = null, bool onlyIfAnySteps = true)
    {
        steps ??= Enumerable.Empty<TimelineStep>();
        if (onlyIfAnySteps && !steps.Any())
        {
            return this;
        }

        var jsonBuilder = new StringBuilder();
        jsonBuilder.AppendLine("[");
        var stepList = steps.ToList();
        for(int i=0; i < stepList.Count; i++)
        {
            var isLast = i == stepList.Count - 1;
            var step = stepList[i];

            var linksJson = "[" + string.Join(", ", step.Links.Select(x => $"[{TKDumpHelper.EncodeForJson(x.Url)}, {TKDumpHelper.EncodeForJson(x.Text)}]")) + "]";
            jsonBuilder.AppendLine($@"
{{
    ""Title"": {TKDumpHelper.EncodeForJson(step.Title)},
    ""Description"": {TKDumpHelper.EncodeForJson(step.Description)},
    ""Links"": {linksJson},
    ""Error"": {TKDumpHelper.EncodeForJson(step.Error)},
    ""Timestamp"": {TKDumpHelper.EncodeForJson(step.Timestamp)},
    ""Icon"": {TKDumpHelper.EncodeForJson(step.Icon)},
    ""HideTimeInTimestamp"": {TKDumpHelper.EncodeForJson(step.HideTimeInTimestamp)},
    ""IsCompleted"": {TKDumpHelper.EncodeForJson(step.IsCompleted)},
    ""Index"": {i}
}}
");
            if (!isLast)
            {
                jsonBuilder.Append(",");
            }
        }
        jsonBuilder.AppendLine("]");

        var json = jsonBuilder.ToString();
        return AddData(json, title, TestResultDataDumpType.Timeline);
    }

    /// <summary>
    /// Include a link to download a file with the given name.
    /// <para>Requires <see cref="TKTestsModuleOptions.FileDownloadHandler"/> to be implemented, that will resolve the given id/type into the actual file contents.</para>
    /// <para>Recommended practice is to make your own custom extension methods that invoke this method, with constant type etc.</para>
    /// <para>Up to the latest 100 download links per session will be active for 10 minutes from the last time the session invoked this method.</para>
    /// </summary>
    /// <param name="id">An id that will be sent to HandleDownloadFileById to select what file to download.</param>
    /// <param name="name">Filename displayed.</param>
    /// <param name="description">Description of the file if any.</param>
    /// <param name="type">Optional value sent to HandleDownloadFileById.</param>
    /// <param name="title">Title of the result data if any.</param>
    public TestResult AddFileDownload(string id, string name, string description = null, string type = null, string title = null)
    {
        var data = $@"
{{
    ""Id"": {TKDumpHelper.EncodeForJson(id)},
    ""Type"": {TKDumpHelper.EncodeForJson(type)},
    ""Name"": {TKDumpHelper.EncodeForJson(name)},
    ""Description"": {TKDumpHelper.EncodeForJson(description)}
}}";

        TKTestsModule.AllowFileDownloadForSession(type, id);
        return AddData(data, title, TestResultDataDumpType.FileDownload);
    }

    /// <summary>
    /// Include the given <see cref="SiteEvent"/>.
    /// <para>Only the data from this object will be included in the overview.</para>
    /// <para>Manual test executions from the UI will not be reported to any overview by default.</para>
    /// </summary>
    public TestResult SetSiteEvent(SiteEvent ev)
    {
        SiteEvent = ev;
        return this;
    }

    /// <summary>
    /// Include a resolved site event, resolving the previous site event with the given <paramref name="eventTypeId"/>.
    /// </summary>
    /// <param name="eventTypeId">Event type id of previous site event to resolve.</param>
    /// <param name="resolvedMessage">Resolved message.</param>
    public TestResult SetResolvedSiteEvent(string eventTypeId, string resolvedMessage)
    {
        SiteEvent = new SiteEvent(eventTypeId, resolvedMessage);
        return this;
    }

    /// <summary>
    /// Attempt to auto-create result data from the given raw data.
    /// <para>Does not change message or include the whole object, this method only attempts to parse out e.g. urls and images and show them.</para>
    /// </summary>
    /// <param name="data">Data to try to create result from.</param>
    /// <param name="include">Set to false to don't do anything.</param>
    public TestResult AddAutoCreatedResultData(object data, bool include = true)
    {
        if (include)
        {
            AutoCreateResultDataFromObject = data;
        }
        return this;
    }

    /// <summary>
    /// Can be used with proxy-test results to clear auto-added data.
    /// </summary>
    public TestResult ClearAutoCreatedResultData()
    {
        AutoCreateResultDataFromObject = null;
        return this;
    }

    /// <summary>
    /// Used by proxy test logic.
    /// </summary>
    internal TestResult SetProxyTestResultObject(object value)
    {
        ProxyTestResultObject = value;
        return this;
    }

    /// <summary>
    /// If <see cref="ProxyTestResultObject"/> is of the given type, execute the given action.
    /// </summary>
    public TestResult ForProxyResult<TResultType>(Action<TResultType> action)
    {
        if (AutoCreateResultDataFromObject is TResultType result)
        {
            action(result);
        }
        return this;
    }
    #endregion

    /// <summary>
    /// Name and severity of the result.
    /// </summary>
    public override string ToString() => $"[{Status}] {Message}";

    private TestResult AddData(string data, string title, TestResultDataDumpType type, bool onlyIfNotNullOrEmpty, string downloadFileName, params string[] flags)
    {
        if (string.IsNullOrWhiteSpace(data) && onlyIfNotNullOrEmpty)
            return this;

        var flagList = flags
            ?.Where(x => x != null)
            ?.ToList()
            ?? new List<string>();

        Data.Add(new TestResultDataDump()
        {
            Title = title,
            Data = data,
            Type = type,
            DownloadFileName = downloadFileName,
            Flags = flagList
        });
        return this;
    }

    private static string CreateDownloadFilename(string customName, string title, TestResultDataDumpType type)
    {
        // Only check for null, since if customName is an empty string we hide the download button.
        if (customName != null)
        {
            return customName;
        }

        var name = TKIOUtils.SanitizeFilename(title ?? $"{type}Data");
        var timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");

        var extension = ".txt";
        if (type == TestResultDataDumpType.Xml) extension = ".xml";
        else if (type == TestResultDataDumpType.Html) extension = ".html";
        else if (type == TestResultDataDumpType.Json) extension = ".json";

        return $"{name}_{timestamp}{extension}";
    }
}
