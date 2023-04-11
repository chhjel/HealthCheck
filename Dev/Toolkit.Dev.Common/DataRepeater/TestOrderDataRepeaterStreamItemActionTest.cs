using QoDL.Toolkit.Core.Modules.DataRepeater.Abstractions;
using QoDL.Toolkit.Core.Modules.DataRepeater.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QoDL.Toolkit.Dev.Common.DataRepeater;

public class TestOrderDataRepeaterStreamItemActionTest : TKDataRepeaterStreamItemActionBase<TestOrderDataRepeaterStreamItemActionTest.Parameters>
{
    public override string DisplayName => "Action test with requirement";

    public override string Description => "Only allowed when processed";

    public override string ExecuteButtonLabel => "Test";

    public override Task<TKDataRepeaterStreamItemActionAllowedResult> ActionIsAllowedForAsync(ITKDataRepeaterStreamItem item)
    {
        var allowedOnItemsWithTags = new List<string> { "Processed", "Random tag", "Something" };
        if (!item.Tags.Any(t => allowedOnItemsWithTags.Contains(t)))
        {
            return Task.FromResult(TKDataRepeaterStreamItemActionAllowedResult.CreateNotAllowed("Missing required tag. Need one of " + string.Join(", ", allowedOnItemsWithTags)));
        }
        return Task.FromResult(TKDataRepeaterStreamItemActionAllowedResult.CreateAllowed());
    }

    protected override Task<TKDataRepeaterStreamItemActionResult> PerformActionAsync(ITKDataRepeaterStream stream, ITKDataRepeaterStreamItem item, Parameters parameters)
    {
        var result = new TKDataRepeaterStreamItemActionResult
        {
            Success = true,
            Message = "Ok!",
            RemoveAllTags = true
        };
        return Task.FromResult(result);
    }

    public class Parameters
    {
    }
}
