using QoDL.Toolkit.Core.Attributes;
using QoDL.Toolkit.Core.Models;
using QoDL.Toolkit.Core.Modules.DataRepeater.Abstractions;
using QoDL.Toolkit.Core.Modules.DataRepeater.Models;
using System.Threading.Tasks;

namespace QoDL.Toolkit.Dev.Common.DataRepeater;

public class TestOrderDataRepeaterStreamBatchActionRenameTag : TKDataRepeaterStreamItemBatchActionBase<TestOrderDataRepeaterStreamBatchActionRenameTag.Parameters>
{
    public override string DisplayName => "Rename tag";

    public override string Description => "Renames a tag on all items.";

    public override string ExecuteButtonLabel => "Rename";

    protected override Task<TKDataRepeaterStreamItemBatchActionResult> PerformBatchActionAsync(ITKDataRepeaterStreamItem item, Parameters parameters, TKDataRepeaterStreamBatchActionResult batchResult)
    {
        if (!item.Tags.Contains(parameters.TagToRename))
        {
            return Task.FromResult(TKDataRepeaterStreamItemBatchActionResult.CreateNotAttemptedUpdated());
        }

        item.Tags.Remove(parameters.TagToRename);
        item.Tags.Add(parameters.NewTagName);

        var shouldStopJob = batchResult.AttemptedUpdatedCount + 1 >= parameters.MaxItemsToUpdate;
        return Task.FromResult(TKDataRepeaterStreamItemBatchActionResult.CreateSuccess(shouldStopJob));
    }

    public class Parameters
    {
        [TKCustomProperty(UIHints = TKUIHint.NotNull)]
        public string TagToRename { get; set; }

        [TKCustomProperty(UIHints = TKUIHint.NotNull)]
        public string NewTagName { get; set; }

        [TKCustomProperty(UIHints = TKUIHint.NotNull)]
        public int MaxItemsToUpdate { get; set; } = 50;
    }
}
