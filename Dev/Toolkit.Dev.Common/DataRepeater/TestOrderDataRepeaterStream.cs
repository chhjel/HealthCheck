using QoDL.Toolkit.Core.Modules.DataRepeater.Abstractions;
using QoDL.Toolkit.Core.Modules.DataRepeater.Models;
using QoDL.Toolkit.WebUI.Services;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace QoDL.Toolkit.Dev.Common.DataRepeater
{
    public class TestOrderDataRepeaterStream : TKDataRepeaterStreamBase<TestOrderStreamItem>
    {
        public override string StreamDisplayName => "Order Captures";
        public override string StreamDescription => "Some description <a href=\"/some/link\">here</a>.";
        public override string StreamGroupName => "Orders";
        public override string StreamItemsName => "Orders";
        public override string ItemIdDisplayName => "Order number";
        public override string RetryActionName => "Retry capture";
        public override string RetryDescription => "Attempts to perform the capture action again.";
        public override string AnalyzeActionName => "Check for errors";
        public override List<string> InitiallySelectedTags => new() { "Failed" };
        public override List<string> FilterableTags => new() { "Failed", "Retried", "Processed" };
        public override List<ITKDataRepeaterStreamItemAction> Actions => new()
        {
            new TestOrderDataRepeaterStreamItemActionModifyData(),
            new TestOrderDataRepeaterStreamItemActionToggleAllow(),
            new TestOrderDataRepeaterStreamItemActionRemoveAllTags(),
            new TestOrderDataRepeaterStreamItemActionTest(),
            new TestOrderDataRepeaterStreamItemActionToggleSuccess()
        };
        public override List<ITKDataRepeaterStreamItemBatchAction> BatchActions => new()
        {
            new TestOrderDataRepeaterStreamBatchActionRenameTag(),
            new TestOrderDataRepeaterStreamBatchActionTest1(),
            new TestOrderDataRepeaterStreamBatchActionTest2(),
            new TestOrderDataRepeaterStreamBatchActionTestAdmin()
        };

        public TestOrderDataRepeaterStream() : base(new TKFlatFileDataRepeaterStreamItemStorage<TestOrderStreamItem>(@"C:\temp\Toolkit\DataRepeater_OrderCaps.json"))
        {
        }

        protected override Task<TKDataRepeaterStreamItemDetails> GetItemDetailsAsync(TestOrderStreamItem item)
        {
            var details = new TKDataRepeaterStreamItemDetails
            {
                DescriptionHtml = "<p>Description here and some more text etc etc with <a href=\"#etc\">some link here.</a></p>",
                Links = new List<TKDataRepeaterStreamItemHyperLink>
                {
                    new TKDataRepeaterStreamItemHyperLink("Test link", "/etc1"),
                    new TKDataRepeaterStreamItemHyperLink("Details page", "/etc2")
                }
            };
            return Task.FromResult(details);
        }

        protected override Task<TKDataRepeaterItemAnalysisResult> AnalyzeItemAsync(TestOrderStreamItem item, bool isManualAnalysis = false)
        {
            var result = new TKDataRepeaterItemAnalysisResult();

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

        protected override Task<TKDataRepeaterRetryResult> RetryItemAsync(TestOrderStreamItem item)
        {
            Debug.WriteLine($"Processed '{item.ItemId}'");

            var result = new TKDataRepeaterRetryResult
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
