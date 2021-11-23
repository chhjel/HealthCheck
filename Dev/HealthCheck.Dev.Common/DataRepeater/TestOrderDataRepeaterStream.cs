using HealthCheck.Core.Modules.DataRepeater.Abstractions;
using HealthCheck.Core.Modules.DataRepeater.Models;
using HealthCheck.Core.Modules.DataRepeater.Storage;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace HealthCheck.Dev.Common.DataRepeater
{
    public class TestOrderDataRepeaterStream : HCDataRepeaterStreamBase<TestOrderStreamItem>
    {
        public override string StreamDisplayName => "Order Captures";
        public override string StreamGroupName => "Orders";
        public override string StreamItemsName => "Orders";
        public override string ItemIdDisplayName => "Order number";
        public override string RetryActionName => "Retry capture";
        public override string RetryDescription => "Attempts to perform the capture action again.";
        public override List<string> InitiallySelectedTags => new List<string> { "Failed" };
        public override List<string> FilterableTags => new List<string> { "Failed", "Something", "Retried" };
        public override List<IHCDataRepeaterStreamItemAction> Actions => new List<IHCDataRepeaterStreamItemAction>
        {
            new TestOrderDataRepeaterStreamItemActionToggleAllow(),
            new TestOrderDataRepeaterStreamItemActionRemoveAllTags(),
            new TestOrderDataRepeaterStreamItemActionTest()
        };

        public TestOrderDataRepeaterStream() : base(new MemoryDataRepeaterStreamItemStorage("ordertest"))
        {
        }

        public override Task<HCDataRepeaterStreamItemDetails> GetItemDetailsAsync(Guid id)
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

        protected override Task<HCDataRepeaterItemAnalysisResult> AnalyzeItemAsync(TestOrderStreamItem item)
        {
            var result = new HCDataRepeaterItemAnalysisResult();

            if (item.ItemId.StartsWith("T"))
            {
                item.AllowRetry = false;
            }

            if (item.ItemId.StartsWith("NoStore"))
            {
                result.DontStore = true;
            }

            if (!item.AllowRetry)
            {
                result.TagsThatShouldExist.Add("No retry allowed");
                result.TagsThatShouldNotExist.Add("Retry allowed");
            }
            else
            {
                result.TagsThatShouldNotExist.Add("No retry allowed");
                result.TagsThatShouldExist.Add("Retry allowed");
            }

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
                TagsThatShouldExist = new List<string> { "Processed" }
            };
            return Task.FromResult(result);
        }
    }
}
