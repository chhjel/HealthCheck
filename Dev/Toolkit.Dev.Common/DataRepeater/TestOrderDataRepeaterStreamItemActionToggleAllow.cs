using QoDL.Toolkit.Core.Attributes;
using QoDL.Toolkit.Core.Modules.DataRepeater.Abstractions;
using QoDL.Toolkit.Core.Modules.DataRepeater.Models;
using System.Threading.Tasks;

namespace QoDL.Toolkit.Dev.Common.DataRepeater;

public class TestOrderDataRepeaterStreamItemActionToggleAllow : TKDataRepeaterStreamItemActionBase<TestOrderDataRepeaterStreamItemActionToggleAllow.Parameters>
{
    public override string DisplayName => "Set allow retry";

    public override string Description => "Sets AllowRetry property";

    public override string ExecuteButtonLabel => "Set";

    protected override Task<TKDataRepeaterStreamItemActionResult> PerformActionAsync(ITKDataRepeaterStream stream, ITKDataRepeaterStreamItem item, Parameters parameters)
    {
        var result = new TKDataRepeaterStreamItemActionResult
        {
            Success = true,
            AllowRetry = parameters.Allowed,
            Message = "Action A performed!"
        };
        return Task.FromResult(result);
    }

    public class Parameters
    {
        [TKCustomProperty]
        public bool Allowed { get; set; }
    }
}
