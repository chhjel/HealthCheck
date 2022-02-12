using HealthCheck.Core.Modules.DataRepeater.Abstractions;
using HealthCheck.Core.Modules.DataRepeater.Models;
using HealthCheck.WebUI.Services;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace HealthCheck.Dev.Common.DataRepeater
{
    public class TestOrderDataRepeaterStream : HCDataRepeaterStreamBase<TestOrderStreamItem>
    {
        public override string StreamDisplayName => "Order Captures";
        public override string StreamDescription => "Some description <a href=\"/some/link\">here</a>.";
        public override string StreamGroupName => "Orders";
        public override string StreamItemsName => "Orders";
        public override string ItemIdDisplayName => "Order number";
        public override string RetryActionName => "Retry capture";
        public override string RetryDescription => "Attempts to perform the capture action again.";
        public override string AnalyzeActionName => "Check for errors";
        public override List<string> InitiallySelectedTags => new List<string> { "Failed" };
        public override List<string> FilterableTags => new List<string> { "Failed", "Retried", "Processed" };
        public override List<IHCDataRepeaterStreamItemAction> Actions => new List<IHCDataRepeaterStreamItemAction>
        {
            new TestOrderDataRepeaterStreamItemActionModifyData(),
            new TestOrderDataRepeaterStreamItemActionToggleAllow(),
            new TestOrderDataRepeaterStreamItemActionRemoveAllTags(),
            new TestOrderDataRepeaterStreamItemActionTest(),
            new TestOrderDataRepeaterStreamItemActionToggleSuccess()
        };
        public override List<IHCDataRepeaterStreamItemBatchAction> BatchActions => new List<IHCDataRepeaterStreamItemBatchAction>()
        {
            new TestOrderDataRepeaterStreamBatchActionRenameTag(),
            new TestOrderDataRepeaterStreamBatchActionTest1(),
            new TestOrderDataRepeaterStreamBatchActionTest2(),
            new TestOrderDataRepeaterStreamBatchActionTestAdmin()
        };

        public TestOrderDataRepeaterStream() : base(new HCFlatFileDataRepeaterStreamItemStorage<TestOrderStreamItem>(@"C:\temp\HealthCheck\DataRepeater_OrderCaps.json"))
        {
        }

        protected override Task<HCDataRepeaterStreamItemDetails> GetItemDetailsAsync(TestOrderStreamItem item)
        {
            var details = new HCDataRepeaterStreamItemDetails
            {
                DescriptionHtml = "<p>Description here and some more text etc etc with <a href=\"#etc\">some link here.</a></p>",
                Links = new List<HCDataRepeaterStreamItemHyperLink>
                {
                    new HCDataRepeaterStreamItemHyperLink("Test link", "/etc1"),
                    new HCDataRepeaterStreamItemHyperLink("Details page", "/etc2")
                }
            };
            return Task.FromResult(details);
        }

        protected override Task<HCDataRepeaterItemAnalysisResult> AnalyzeItemAsync(TestOrderStreamItem item, bool isManualAnalysis = false)
        {
            var result = new HCDataRepeaterItemAnalysisResult();

            if (!string.IsNullOrWhiteSpace(item.Error) && item.LastRetryWasSuccessful != true)
            {
                item.AllowRetry = true;
            }
            else
            {
                item.AllowRetry = false;
            }
            item.SetTag("Failed", item.AllowRetry);

            result.Message = $"Analysis complete! Allow retry was decided {item.AllowRetry}";
            return Task.FromResult(result);
        }

        protected override Task<HCDataRepeaterRetryResult> RetryItemAsync(TestOrderStreamItem item)
        {
            Debug.WriteLine($"Processed '{item.ItemId}'");

            var result = new HCDataRepeaterRetryResult
            {
                Success = true,
                Message = $"Success! New #{item.Data.OrderNumber}, ${item.Data.Amount}",

                AllowRetry = false,
                Delete = false,
                RemoveAllTags = true,
                TagsThatShouldExist = new List<string> { "Retried", "Processed" }
            };
            return Task.FromResult(result);
        }
    }
}
