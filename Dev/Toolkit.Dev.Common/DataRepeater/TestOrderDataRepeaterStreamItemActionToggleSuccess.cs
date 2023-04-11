using QoDL.Toolkit.Core.Attributes;
using QoDL.Toolkit.Core.Modules.DataRepeater.Abstractions;
using QoDL.Toolkit.Core.Modules.DataRepeater.Models;
using System.Threading.Tasks;

namespace QoDL.Toolkit.Dev.Common.DataRepeater
{
    public class TestOrderDataRepeaterStreamItemActionToggleSuccess : TKDataRepeaterStreamItemActionBase<TestOrderDataRepeaterStreamItemActionToggleSuccess.Parameters>
    {
        public override string DisplayName => "Set successful";

        public override string Description => "Marks the item as successful or not";

        public override string ExecuteButtonLabel => "Set";

        protected override Task<TKDataRepeaterStreamItemActionResult> PerformActionAsync(ITKDataRepeaterStream stream, ITKDataRepeaterStreamItem item, Parameters parameters)
        {
            var result = TKDataRepeaterStreamItemActionResult.CreateSuccess($"Marked as {(parameters.Successful ? "successful" : "unsuccessful")}");

            result.SetStreamItemModification<TestOrderStreamItem>(streamItem =>
            {
                streamItem.ForcedStatus = parameters.Successful ? TKDataRepeaterStreamItemStatus.Success : TKDataRepeaterStreamItemStatus.Error;
            });

            return Task.FromResult(result);
        }

        public class Parameters
        {
            [TKCustomProperty]
            public bool Successful { get; set; }
        }
    }
}
