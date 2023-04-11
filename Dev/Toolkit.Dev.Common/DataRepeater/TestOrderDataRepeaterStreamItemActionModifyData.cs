using QoDL.Toolkit.Core.Modules.DataRepeater.Abstractions;
using QoDL.Toolkit.Core.Modules.DataRepeater.Models;
using System.Threading.Tasks;

namespace QoDL.Toolkit.Dev.Common.DataRepeater
{
    public class TestOrderDataRepeaterStreamItemActionModifyData : TKDataRepeaterStreamItemActionBase<TestOrderDataRepeaterStreamItemActionModifyData.Parameters>
    {
        public override string DisplayName => "Update data";

        public override string Description => "Updates data to see that it works.";

        public override string ExecuteButtonLabel => "Update";

        protected override Task<TKDataRepeaterStreamItemActionResult> PerformActionAsync(ITKDataRepeaterStream stream, ITKDataRepeaterStreamItem item, Parameters parameters)
        {
            var result = TKDataRepeaterStreamItemActionResult.CreateSuccess("Data updated in the input.");

            result.SetStreamItemModification<TestOrderStreamItem>(streamItem =>
            {
                streamItem.ForcedStatus = TKDataRepeaterStreamItemStatus.Error;
                streamItem.ModifyData(d => d.OrderNumber += "+");
            });

            return Task.FromResult(result);
        }

        public class Parameters
        {
        }
    }
}
