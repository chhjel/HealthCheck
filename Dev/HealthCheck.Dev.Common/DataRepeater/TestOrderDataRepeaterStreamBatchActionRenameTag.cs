using HealthCheck.Core.Attributes;
using HealthCheck.Core.Models;
using HealthCheck.Core.Modules.DataRepeater.Abstractions;
using HealthCheck.Core.Modules.DataRepeater.Models;
using System.Threading.Tasks;

namespace HealthCheck.Dev.Common.DataRepeater
{
    public class TestOrderDataRepeaterStreamBatchActionRenameTag : HCDataRepeaterStreamItemBatchActionBase<TestOrderDataRepeaterStreamBatchActionRenameTag.Parameters>
    {
        public override string DisplayName => "Rename tag";

        public override string Description => "Renames a tag on all items.";

        public override string ExecuteButtonLabel => "Rename";

        protected override Task<HCDataRepeaterStreamItemBatchActionResult> PerformBatchActionAsync(IHCDataRepeaterStreamItem item, Parameters parameters, HCDataRepeaterStreamBatchActionResult batchResult)
        {
            if (!item.Tags.Contains(parameters.TagToRename))
            {
                return Task.FromResult(HCDataRepeaterStreamItemBatchActionResult.CreateNotAttemptedUpdated());
            }

            item.Tags.Remove(parameters.TagToRename);
            item.Tags.Add(parameters.NewTagName);

            var shouldStopJob = batchResult.AttemptedUpdatedCount + 1 >= parameters.MaxItemsToUpdate;
            return Task.FromResult(HCDataRepeaterStreamItemBatchActionResult.CreateSuccess(shouldStopJob));
        }

        public class Parameters
        {
            [HCCustomProperty(UIHints = HCUIHint.NotNull)]
            public string TagToRename { get; set; }

            [HCCustomProperty(UIHints = HCUIHint.NotNull)]
            public string NewTagName { get; set; }

            [HCCustomProperty(UIHints = HCUIHint.NotNull)]
            public int MaxItemsToUpdate { get; set; } = 50;
        }
    }
}
