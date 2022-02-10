using HealthCheck.Core.Attributes;
using HealthCheck.Core.Modules.DataRepeater.Abstractions;
using HealthCheck.Core.Modules.DataRepeater.Models;
using System.Threading.Tasks;

namespace HealthCheck.Dev.Common.DataRepeater
{
    public class TestOrderDataRepeaterStreamItemActionToggleSuccess : HCDataRepeaterStreamItemActionBase<TestOrderDataRepeaterStreamItemActionToggleSuccess.Parameters>
    {
        public override string DisplayName => "Set successful";

        public override string Description => "Marks the item as successful or not";

        public override string ExecuteButtonLabel => "Set";

        protected override Task<HCDataRepeaterStreamItemActionResult> PerformActionAsync(IHCDataRepeaterStream stream, IHCDataRepeaterStreamItem item, Parameters parameters)
        {
            var result = HCDataRepeaterStreamItemActionResult.CreateSuccess($"Marked as {(parameters.Successful ? "successful" : "unsuccessful")}");

            result.SetStreamItemModification<TestOrderStreamItem>(streamItem =>
            {
                streamItem.ForcedStatus = parameters.Successful ? HCDataRepeaterStreamItemStatus.Success : HCDataRepeaterStreamItemStatus.Error;
            });

            return Task.FromResult(result);
        }

        public class Parameters
        {
            [HCCustomProperty]
            public bool Successful { get; set; }
        }
    }
}
