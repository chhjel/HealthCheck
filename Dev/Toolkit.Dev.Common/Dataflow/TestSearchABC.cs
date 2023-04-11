using QoDL.Toolkit.Core.Modules.Dataflow.Abstractions;
using QoDL.Toolkit.Core.Modules.Dataflow.Models;
using QoDL.Toolkit.Core.Util;
using System;
using System.Collections.Generic;

namespace QoDL.Toolkit.Dev.Common.Dataflow;

public class TestSearchABC : ITKDataflowUnifiedSearch<RuntimeTestAccessRole>
{
    public Maybe<RuntimeTestAccessRole> RolesWithAccess => null;
    public string Name { get; } = "ABC Search";
    public string Description { get; } = "Searches A, B and C test streams.";
    public string QueryPlaceholder { get; } = "Search..";
    public string GroupName { get; } = "Searches";
    public string GroupByLabel { get; }
    public Dictionary<Type, string> StreamNamesOverrides { get; }
    public Dictionary<Type, string> GroupByStreamNamesOverrides { get; }
    public Func<bool> IsVisible { get; } = () => true;
    public IEnumerable<Type> StreamTypesToSearch { get; } = new[] { typeof(TestStreamA), typeof(TestStreamB), typeof(TestStreamC) };

    public Dictionary<string, string> CreateStreamPropertyFilter(IDataflowStream<RuntimeTestAccessRole> stream, string query)
    {
        var filter = new Dictionary<string, string>();
        if (stream.GetType() == typeof(TestStreamA)) filter[nameof(TestEntry.Code)] = query;
        else if (stream.GetType() == typeof(TestStreamB)) filter[nameof(TestEntry.Code)] = query;
        else if (stream.GetType() == typeof(TestStreamC)) filter[nameof(TestEntry.Code)] = query;
        return filter;
    }

    public TKDataflowUnifiedSearchResultItem CreateResultItem(Type streamType, IDataflowEntry entry)
    {
        var item = entry as TestEntry;
        var result = new TKDataflowUnifiedSearchResultItem
        {
            Title = item.Name,
            Body = item.HtmlTest
        };
        if (!item.Name.Contains("A4888")) result.TryCreatePopupBodyFrom(item);
        return result;
    }
}
