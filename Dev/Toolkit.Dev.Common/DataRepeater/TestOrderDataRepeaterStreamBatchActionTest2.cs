using QoDL.Toolkit.Core.Modules.DataRepeater.Abstractions;
using QoDL.Toolkit.Core.Modules.DataRepeater.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace QoDL.Toolkit.Dev.Common.DataRepeater;

public class TestOrderDataRepeaterStreamBatchActionTest2 : TKDataRepeaterStreamItemBatchActionBase<TestOrderDataRepeaterStreamBatchActionTest2.Parameters>
{
    public override string DisplayName => "Some action #2";

    public override string Description => "Does something but who knows what, it's just for testing the UI.";

    public override string ExecuteButtonLabel => "Execute the thing";

    public override List<string> Categories => new() { "Test 2" };

    protected override Task<TKDataRepeaterStreamItemBatchActionResult> PerformBatchActionAsync(ITKDataRepeaterStreamItem item, Parameters parameters, TKDataRepeaterStreamBatchActionResult batchResult)
    {
        return Task.FromResult(TKDataRepeaterStreamItemBatchActionResult.CreateNotAttemptedUpdated());
    }

    public class Parameters
    {
    }
}
