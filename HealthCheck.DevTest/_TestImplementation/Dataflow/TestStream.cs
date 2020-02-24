using HealthCheck.Core.Modules.Dataflow;
using HealthCheck.Core.Util;
using HealthCheck.WebUI.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HealthCheck.DevTest._TestImplementation.Dataflow
{
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

            RegisterPropertyDisplayInfo(new DataFlowPropertyDisplayInfo(nameof(TestEntry.Code))
            {
                DisplayName = "The product code",
                UIOrder = 0,
                IsFilterable = true
            });
            RegisterPropertyDisplayInfo(new DataFlowPropertyDisplayInfo(nameof(TestEntry.Name))
            {
                DisplayName = "The product name",
                UIOrder = 1,
                IsFilterable = true
            });
            RegisterPropertyDisplayInfo(new DataFlowPropertyDisplayInfo(nameof(TestEntry.InsertionTime))
            {
                UIHint = DataFlowPropertyDisplayInfo.DataFlowPropertyUIHint.DateTime,
                UIOrder = 2
            });
            RegisterPropertyDisplayInfo(new DataFlowPropertyDisplayInfo(nameof(TestEntry.PreformattedTest))
            {
                UIHint = DataFlowPropertyDisplayInfo.DataFlowPropertyUIHint.Preformatted,
                UIOrder = 3
            });
            RegisterPropertyDisplayInfo(new DataFlowPropertyDisplayInfo(nameof(TestEntry.HtmlTest))
            {
                UIHint = DataFlowPropertyDisplayInfo.DataFlowPropertyUIHint.HTML,
                UIOrder = 4
            });
            RegisterPropertyDisplayInfo(new DataFlowPropertyDisplayInfo(nameof(TestEntry.Properties))
            {
                UIHint = DataFlowPropertyDisplayInfo.DataFlowPropertyUIHint.Dictionary,
                Visibility = DataFlowPropertyDisplayInfo.DataFlowPropertyUIVisibilityOption.OnlyWhenExpanded
            });
            RegisterPropertyDisplayInfo(new DataFlowPropertyDisplayInfo(nameof(TestEntry.TestList))
            {
                UIHint = DataFlowPropertyDisplayInfo.DataFlowPropertyUIHint.List,
                Visibility = DataFlowPropertyDisplayInfo.DataFlowPropertyUIVisibilityOption.OnlyWhenExpanded
            });
            RegisterPropertyDisplayInfo(new DataFlowPropertyDisplayInfo(nameof(TestEntry.TestLink))
            {
                UIHint = DataFlowPropertyDisplayInfo.DataFlowPropertyUIHint.Link,
                Visibility = DataFlowPropertyDisplayInfo.DataFlowPropertyUIVisibilityOption.OnlyWhenExpanded,
                IsFilterable = true
            });
            RegisterPropertyDisplayInfo(new DataFlowPropertyDisplayInfo(nameof(TestEntry.TestImage))
            {
                UIHint = DataFlowPropertyDisplayInfo.DataFlowPropertyUIHint.Image,
                Visibility = DataFlowPropertyDisplayInfo.DataFlowPropertyUIVisibilityOption.OnlyWhenExpanded
            });
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
}