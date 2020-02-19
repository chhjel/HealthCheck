using HealthCheck.Core.Modules.Dataflow;
using HealthCheck.WebUI.Services;
using System;

namespace HealthCheck.DevTest._TestImplementation.Dataflow
{
    public class TestStream : FlatFileStoredDataflowStream<TestEntry, string>
    {
        public override string Id => $"dev_stream_{Suffix}";
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
            )
        {
            Suffix = suffix;

            RegisterPropertyDisplayInfo(new DataFlowPropertyDisplayInfo(nameof(TestEntry.Code))
            {
                DisplayName = "The product code",
                UIOrder = 0
            });
            RegisterPropertyDisplayInfo(new DataFlowPropertyDisplayInfo(nameof(TestEntry.Name))
            {
                DisplayName = "The product name",
                UIOrder = 1
            });
            RegisterPropertyDisplayInfo(new DataFlowPropertyDisplayInfo(nameof(TestEntry.InsertionTime))
            {
                UIHint = DataFlowPropertyDisplayInfo.DataFlowPropertyUIHint.DateTime,
                UIOrder = 2
            });
            RegisterPropertyDisplayInfo(new DataFlowPropertyDisplayInfo(nameof(TestEntry.Properties))
            {
                UIHint = DataFlowPropertyDisplayInfo.DataFlowPropertyUIHint.Raw,
                Visibility = DataFlowPropertyDisplayInfo.DataFlowPropertyUIVisibilityOption.OnlyWhenExpanded
            });
        }
    }
}