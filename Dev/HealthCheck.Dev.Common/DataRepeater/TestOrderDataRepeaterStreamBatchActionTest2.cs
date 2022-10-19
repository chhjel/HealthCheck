using HealthCheck.Core.Modules.DataRepeater.Abstractions;
using HealthCheck.Core.Modules.DataRepeater.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HealthCheck.Dev.Common.DataRepeater
{
    public class TestOrderDataRepeaterStreamBatchActionTest2 : HCDataRepeaterStreamItemBatchActionBase<TestOrderDataRepeaterStreamBatchActionTest2.Parameters>
    {
        public override string DisplayName => "Some action #2";

        public override string Description => "Does something but who knows what, it's just for testing the UI.";

        public override string ExecuteButtonLabel => "Execute the thing";

        public override List<string> Categories => new() { "Test 2" };

        protected override Task<HCDataRepeaterStreamItemBatchActionResult> PerformBatchActionAsync(IHCDataRepeaterStreamItem item, Parameters parameters, HCDataRepeaterStreamBatchActionResult batchResult)
        {
            return Task.FromResult(HCDataRepeaterStreamItemBatchActionResult.CreateNotAttemptedUpdated());
        }

        public class Parameters
        {
        }
    }
}
