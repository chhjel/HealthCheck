using HealthCheck.Core.Modules.Dataflow.Abstractions;
using HealthCheck.Core.Modules.Dataflow.Models;
using HealthCheck.Core.Util;
using System;
using System.Collections.Generic;

namespace HealthCheck.Dev.Common.Dataflow
{
    public class TestSearchABC : IHCDataflowUnifiedSearch<RuntimeTestAccessRole>
    {
        public Maybe<RuntimeTestAccessRole> RolesWithAccess => null;
        public string Name => "ABC Search";
        public string Description => "Searches A, B and C test streams.";
        public string QueryPlaceholder => "Search..";
        public string GroupName => "Searches";
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

        public HCDataflowUnifiedSearchResultItem CreateResultItem(IDataflowEntry entry)
        {
            var item = entry as TestEntry;
            var result = new HCDataflowUnifiedSearchResultItem
            {
                Title = item.Name,
                Body = item.HtmlTest
            };
            if (!item.Name.Contains("A4888")) result.TryCreatePopupBodyFrom(item);
            return result;
        }
    }
}
