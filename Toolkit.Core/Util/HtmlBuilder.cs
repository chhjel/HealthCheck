using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace QoDL.Toolkit.Core.Util;

internal static class HtmlBuilder
{
    internal static string BuildAttributes(Dictionary<string, string> values)
    {
        var attributes = values
            .Where(x => !string.IsNullOrWhiteSpace(x.Key) && !string.IsNullOrWhiteSpace(x.Value));
        var builder = new StringBuilder();
        foreach (var a in attributes)
        {
            builder.Append($"{a.Key}=\"{EscapeAttributeValue(a.Value)}\"");
        }
        return builder.ToString();
    }

    internal static string EscapeAttributeValue(string value)
        => HttpUtility.HtmlAttributeEncode(value);
}
