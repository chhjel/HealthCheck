using QoDL.Toolkit.Core.Modules.Dataflow.Abstractions;
using System;
using static QoDL.Toolkit.Core.Modules.Dataflow.Models.DataFlowPropertyDisplayInfo;

namespace QoDL.Toolkit.Dev.Common.Dataflow
{
    public class TestMemoryStream : MemoryDataflowStream<RuntimeTestAccessRole, TestMemoryStreamItem>
    {
        public override string Name => "TestMemoryStream";
        public override string Description => "asd dgsdkg";
        public override string GroupName => _groupName;
        public override string Id => _id;
        private readonly string _groupName;
        private readonly string _id;

        public TestMemoryStream(string groupName)
            : base(maxItemCount: 20, maxDuration: TimeSpan.FromSeconds(30), idSetter: (x, id) => x.Id = id)
        {
            ConfigureProperty("Id").SetVisibility(DataFlowPropertyUIVisibilityOption.Hidden);
            ConfigureProperty("InsertionTime").SetUIHint(DataFlowPropertyUIHint.DateTime).SetUIOrder(0);
            _groupName = groupName;
            _id = Guid.NewGuid().ToString();
        }
    }
}