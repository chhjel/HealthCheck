using HealthCheck.Core.Modules.DataRepeater.Abstractions;
using HealthCheck.Core.Modules.DataRepeater.Models;
using System.Threading.Tasks;

namespace HealthCheck.Dev.Common.DataRepeater
{
    public class TestOrderDataRepeaterStreamItemActionModifyData : HCDataRepeaterStreamItemActionBase<TestOrderDataRepeaterStreamItemActionModifyData.Parameters>
    {
        public override string DisplayName => "Update data";

        public override string Description => "Updates data to see that it works.";

        public override string ExecuteButtonLabel => "Update";

        protected override Task<HCDataRepeaterStreamItemActionResult> PerformActionAsync(IHCDataRepeaterStream stream, IHCDataRepeaterStreamItem item, Parameters parameters)
        {
            var result = HCDataRepeaterStreamItemActionResult.CreateSuccess("Data updated in the input.");

            result.SetStreamItemModification<TestOrderStreamItem>(streamItem =>
            {
                streamItem.ForcedStatus = HCDataRepeaterStreamItemStatus.Error;
                streamItem.ModifyData(d => d.OrderNumber += "+");
            });

            return Task.FromResult(result);
        }

        public class Parameters
        {
        }
    }
}
