using QoDL.Toolkit.Core.Attributes;
using QoDL.Toolkit.Core.Modules.DataRepeater.Abstractions;
using QoDL.Toolkit.Core.Modules.DataRepeater.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace QoDL.Toolkit.Dev.Common.DataRepeater;

public class TestOrderDataRepeaterStreamBatchActionTest1 : TKDataRepeaterStreamItemBatchActionBase<TestOrderDataRepeaterStreamBatchActionTest1.Parameters>
{
    public override string DisplayName => "Some action #1";

    public override string Description => "Does something but who knows what, it's just for testing the UI.";

    public override string ExecuteButtonLabel => "Execute the thing";

    public override List<string> Categories => new() { "Test 1" };

    protected override Task<TKDataRepeaterStreamItemBatchActionResult> PerformBatchActionAsync(ITKDataRepeaterStreamItem item, Parameters parameters, TKDataRepeaterStreamBatchActionResult batchResult)
    {
        return Task.FromResult(TKDataRepeaterStreamItemBatchActionResult.CreateNotAttemptedUpdated());
    }

    public class Parameters
    {
        [TKCustomProperty]
        public bool SomeBool { get; set; }

        [TKCustomProperty]
        public DateTimeOffset AtGivenTime { get; set; }
    }
}
