using QoDL.Toolkit.Core.Modules.DataRepeater.Abstractions;
using QoDL.Toolkit.Core.Modules.DataRepeater.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace QoDL.Toolkit.Dev.Common.DataRepeater;

public class TestOrderDataRepeaterStreamItemActionRemoveAllTags : TKDataRepeaterStreamItemActionBase<TestOrderDataRepeaterStreamItemActionRemoveAllTags.Parameters>
{
    public override string DisplayName => "Remove all tags";

    public override string Description => "Removes all tags from the item.";

    public override string ExecuteButtonLabel => "Remove tags";
    
    public override object AllowedAccessRoles => RuntimeTestAccessRole.SystemAdmins;

    public override List<string> Categories => new() { "Secret actions" };

    protected override Task<TKDataRepeaterStreamItemActionResult> PerformActionAsync(ITKDataRepeaterStream stream, ITKDataRepeaterStreamItem item, Parameters parameters)
    {
        var result = new TKDataRepeaterStreamItemActionResult
        {
            Success = true,
            Message = "Tags removed!",
            RemoveAllTags = true
        };
        return Task.FromResult(result);
    }

    public class Parameters
    {
    }
}
