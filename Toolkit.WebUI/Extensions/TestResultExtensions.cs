using QoDL.Toolkit.Core.Modules.Tests.Models;
using QoDL.Toolkit.Core.Modules.Tests.Utils.HtmlPresets;
using QoDL.Toolkit.WebUI.Serializers;

namespace QoDL.Toolkit.WebUI.Extensions;

/// <summary>
/// WebUI extensions for <see cref="TestResult"/>.
/// </summary>
public static class TestResultExtensions
{
    private static readonly NewtonsoftJsonSerializer _serializer = new();

    /// <summary>
    /// Include a json serialized version of the given object in the result data.
    /// </summary>
    public static TestResult AddSerializedData(this TestResult testResult, object data, string title = null)
        => testResult.AddSerializedData(data, _serializer, title);

    /// <summary>
    /// Include html preset data.
    /// </summary>
    /// <param name="testResult">The result to add data to.</param>
    /// <param name="htmlBuilder">Source of the html.</param>
    /// <param name="title">Title of the result data if any.</param>
    /// <param name="onlyIfNotNullOrEmpty">Only include the result if the data is not null or empty.</param>
    /// <param name="downloadFileName">Optionally specify a filename, if given a download button will be shown where the html can be downloaded.</param>
    /// <param name="useShadowDom">Wraps the output in a shadow DOM.</param>
    public static TestResult AddHtmlData(this TestResult testResult, HtmlPresetBuilder htmlBuilder, string title = null, bool onlyIfNotNullOrEmpty = true, string downloadFileName = null, bool useShadowDom = false)
        => testResult.AddHtmlData(htmlBuilder?.ToHtml(), title, onlyIfNotNullOrEmpty, downloadFileName, useShadowDom);
}
