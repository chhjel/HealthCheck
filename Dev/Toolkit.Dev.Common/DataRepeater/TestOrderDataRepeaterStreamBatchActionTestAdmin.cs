using QoDL.Toolkit.Core.Modules.DataRepeater.Abstractions;
using QoDL.Toolkit.Core.Modules.DataRepeater.Models;
using System.Threading.Tasks;

namespace QoDL.Toolkit.Dev.Common.DataRepeater
{
    public class TestOrderDataRepeaterStreamBatchActionTestAdmin : TKDataRepeaterStreamItemBatchActionBase<TestOrderDataRepeaterStreamBatchActionTestAdmin.Parameters>
    {
        public override string DisplayName => "Action for admins";
        public override string Description => "Requires admin access.";
        public override string ExecuteButtonLabel => "Execute the thing";
        public override object AllowedAccessRoles => RuntimeTestAccessRole.SystemAdmins;

        protected override Task<TKDataRepeaterStreamItemBatchActionResult> PerformBatchActionAsync(ITKDataRepeaterStreamItem item, Parameters parameters, TKDataRepeaterStreamBatchActionResult batchResult)
        {
            return Task.FromResult(TKDataRepeaterStreamItemBatchActionResult.CreateNotAttemptedUpdated());
        }

        public class Parameters
        {
        }
    }
}
