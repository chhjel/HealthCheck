using HealthCheck.Core.Modules.Dataflow.Abstractions;
using HealthCheck.Core.Modules.Dataflow.Models;
using HealthCheck.Core.Util;
using System;
using System.Collections.Generic;

namespace HealthCheck.Dev.Common.Dataflow
{
    public class TestSearchABCGrouped : IHCDataflowUnifiedSearch<RuntimeTestAccessRole>
    {
        public Maybe<RuntimeTestAccessRole> RolesWithAccess => null;
        public string Name { get; } = "ABC Search Grouped";
        public string Description { get; } = "Searches A, B and C test streams.";
        public string QueryPlaceholder { get; } = "Search..";
        public string GroupName { get; } = "Searches";
        public string GroupByLabel { get; } = "Item #[KEY]";
        public Dictionary<Type, string> StreamNamesOverrides { get; }
        public Dictionary<Type, string> GroupByStreamNamesOverrides { get; } = new Dictionary<Type, string>() {
            { typeof(TestStreamA), "Product" },
            { typeof(TestStreamB), "Inventory" },
            { typeof(TestStreamC), "Price" }
        };
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

        public HCDataflowUnifiedSearchResultItem CreateResultItem(Type streamType, IDataflowEntry entry)
        {
            var item = entry as TestEntry;
            var result = new HCDataflowUnifiedSearchResultItem
            {
                Title = item.Name,
                Body = item.HtmlTest,
                GroupByKey = item.Code.Substring(1, item.Code.IndexOf("-") - 1)
            };
            if (!item.Name.Contains("A4888")) result.TryCreatePopupBodyFrom(item);
            return result;
        }
    }
}
