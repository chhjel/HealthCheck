using HealthCheck.Core.Modules.Dataflow;
using HealthCheck.Core.Util;
using HealthCheck.WebUI.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static HealthCheck.Core.Modules.Dataflow.DataFlowPropertyDisplayInfo;

namespace HealthCheck.DevTest._TestImplementation.Dataflow
{
    public class TestMemoryStreamItem : IDataflowEntryWithInsertionTime
    {
        public Guid Id { get; set; }
        public string Message { get; set; }
        public DateTime? InsertionTime { get; set; }

        public static implicit operator TestMemoryStreamItem(string message)
            => new TestMemoryStreamItem { Message = message };
    }

    public class TestMemoryStream : MemoryDataflowStream<RuntimeTestAccessRole, TestMemoryStreamItem>
    {
        public override string Name => "TestMemoryStream";
        public override string Description => "asd dgsdkg";

        public TestMemoryStream()
            : base(maxItemCount: 20, maxDuration: TimeSpan.FromSeconds(30), idSetter: (x, id) => x.Id = id)
        {
            ConfigureProperty("Id").SetVisibility(DataFlowPropertyUIVisibilityOption.Hidden);
            ConfigureProperty("InsertionTime").SetUIHint(DataFlowPropertyUIHint.DateTime).SetUIOrder(0);
        }
    }

    public class TestStreamA : TestStream { public TestStreamA() : base("A") { } }
    public class TestStreamB : TestStream { public TestStreamB() : base("B") { } }
    public class TestStreamC : TestStream { public TestStreamC() : base("C") { DateTimePropertyNameForUI = null; } }
    public class TestStream : FlatFileStoredDataflowStream<RuntimeTestAccessRole, TestEntry, string>
    {
        public override Maybe<RuntimeTestAccessRole> RolesWithAccess => (Suffix == "A") ? new Maybe<RuntimeTestAccessRole>(RuntimeTestAccessRole.SystemAdmins) : null;
        public override string Name => $"Dev Stream {Suffix}";
        public override string Description => $"Description for stream '{Name}'.";
        public override bool SupportsFilterByDate => (Suffix == "A");
        private string Suffix { get; }

        public TestStream(string suffix)
            : base(
                @"c:\temp\teststream_" + suffix + ".json",
                idSelector: (e) => e.Code,
                idSetter: (e, id) => e.Code = id,
                maxEntryAge: TimeSpan.FromDays(1)
                //isEnabled: () => someConfigService.EnableMyStream
            )
        {
            Suffix = suffix;

            ConfigureProperty(nameof(TestEntry.Code)).SetDisplayName("The product code").SetFilterable();
            ConfigureProperty(nameof(TestEntry.Name)).SetDisplayName("The product name").SetFilterable();
            ConfigureProperty(nameof(TestEntry.InsertionTime)).SetUIHint(DataFlowPropertyUIHint.DateTime).PrettifyDisplayName();
            ConfigureProperty(nameof(TestEntry.PreformattedTest)).SetUIHint(DataFlowPropertyUIHint.Preformatted);
            ConfigureProperty(nameof(TestEntry.HtmlTest)).SetUIHint(DataFlowPropertyUIHint.HTML);
            ConfigureProperty(nameof(TestEntry.Properties)).SetUIHint(DataFlowPropertyUIHint.Dictionary).SetVisibility(DataFlowPropertyUIVisibilityOption.OnlyWhenExpanded);
            ConfigureProperty(nameof(TestEntry.TestList)).SetUIHint(DataFlowPropertyUIHint.List).SetVisibility(DataFlowPropertyUIVisibilityOption.OnlyWhenExpanded);
            ConfigureProperty(nameof(TestEntry.TestLink)).SetUIHint(DataFlowPropertyUIHint.Link).SetVisibility(DataFlowPropertyUIVisibilityOption.OnlyWhenExpanded);
            ConfigureProperty(nameof(TestEntry.TestImage)).SetUIHint(DataFlowPropertyUIHint.Image).SetVisibility(DataFlowPropertyUIVisibilityOption.OnlyWhenExpanded);
        }

        protected override Task<IEnumerable<TestEntry>> FilterEntries(DataflowStreamFilter filter, IEnumerable<TestEntry> entries)
        {
            // Get user input for Code property
            var codeFilter = filter.GetPropertyFilterInput(nameof(TestEntry.Code));
            // Filter on property
            entries = entries.Where(x => codeFilter == null || x.Code.ToLower().Contains(codeFilter));

            entries = filter.FilterContains(entries, nameof(TestEntry.Name), x => x.Name);

            return Task.FromResult(entries);
        }
    }

    public class SimpleStream : FlatFileStoredDataflowStream<RuntimeTestAccessRole, GenericDataflowStreamObject, string>
    {
        public override string Name => $"Simple Stream {Suffix}";
        public override string Description => $"Description for simple stream '{Name}'.";
        public override bool SupportsFilterByDate => true;
        private string Suffix { get; }

        public SimpleStream(string suffix)
            : base(
                @"c:\temp\simplestream_" + suffix + ".json",
                idSelector: (e) => e.Get<string>(nameof(TestEntry.Code)),
                idSetter: (e, id) => e[nameof(TestEntry.Code)] = id,
                maxEntryAge: TimeSpan.FromDays(1)
            )
        {
            Suffix = suffix;

            AutoCreateFilters<TestEntry>();
            ConfigureProperty(nameof(TestEntry.Code)).SetDisplayName("Custom Code Display Name");
        }

        protected override async Task<IEnumerable<GenericDataflowStreamObject>> FilterEntries(DataflowStreamFilter filter, IEnumerable<GenericDataflowStreamObject> entries)
        {
            entries = filter.FilterContains(entries, nameof(TestEntry.Code), x => x.Get<string>(nameof(TestEntry.Code)));
            return await base.FilterEntries(filter, entries);
        }
    }
}