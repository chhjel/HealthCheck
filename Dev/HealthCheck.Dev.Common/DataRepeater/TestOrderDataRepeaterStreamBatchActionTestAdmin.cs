using HealthCheck.Core.Modules.DataRepeater.Abstractions;
using HealthCheck.Core.Modules.DataRepeater.Models;
using System.Threading.Tasks;

namespace HealthCheck.Dev.Common.DataRepeater
{
    public class TestOrderDataRepeaterStreamBatchActionTestAdmin : HCDataRepeaterStreamItemBatchActionBase<TestOrderDataRepeaterStreamBatchActionTestAdmin.Parameters>
    {
        public override string DisplayName => "Action for admins";
        public override string Description => "Requires admin access.";
        public override string ExecuteButtonLabel => "Execute the thing";
        public override object AllowedAccessRoles => RuntimeTestAccessRole.SystemAdmins;

        protected override Task<HCDataRepeaterStreamItemBatchActionResult> PerformBatchActionAsync(IHCDataRepeaterStreamItem item, Parameters parameters, HCDataRepeaterStreamBatchActionResult batchResult)
        {
            return Task.FromResult(HCDataRepeaterStreamItemBatchActionResult.CreateNotAttemptedUpdated());
        }

        public class Parameters
        {
        }
    }
}
