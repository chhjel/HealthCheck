using HealthCheck.WebUI.Services;
using System;

namespace HealthCheck.DevTest._TestImplementation.Dataflow
{
    public class TestStream : FlatFileStoredDataflowStream<TestEntry, string>
    {
        public override string Name => $"Dev Stream {Suffix}";
        public override string Id => $"dev_stream_{Suffix}";
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
        }
    }
}