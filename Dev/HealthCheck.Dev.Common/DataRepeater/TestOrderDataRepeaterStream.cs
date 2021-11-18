using HealthCheck.Core.Modules.DataRepeater.Abstractions;
using HealthCheck.Core.Modules.DataRepeater.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace HealthCheck.Dev.Common.DataRepeater
{
    public class TestOrderDataRepeaterStream : HCDataRepeaterStreamBase
    {
        public override string StreamDisplayName => "Order Captures";
        public override string StreamGroupName => "Orders";
        public override string ItemIdDisplayName => "Order number";
        public override List<string> InitiallySelectedTags => new List<string> { "Retry allowed" };

        private static Dictionary<Guid, IHCDataRepeaterStreamItem> _items = new Dictionary<Guid, IHCDataRepeaterStreamItem>();

        public override Task DeleteItemAsync(Guid id, string itemId)
        {
            _items.Remove(id);
            return Task.CompletedTask;
        }

        public override Task<IHCDataRepeaterStreamItem> GetItemAsync(Guid id, string itemId)
        {
            var item = _items.ContainsKey(id) ? _items[id] : null;
            return Task.FromResult(item);
        }

        public override Task<HCDataRepeaterStreamItemsPagedModel> GetItemsPagedAsync(HCGetDataRepeaterStreamItemsFilteredRequest model)
        {
            var items = _items.Values
                .Where(x => x.ItemId.Contains(model.ItemId) || x.Tags.Any(t => model.Tags.Contains(t)))
                .Skip(model.PageIndex)
                .Take(model.PageSize);
            var result = new HCDataRepeaterStreamItemsPagedModel
            {
                TotalCount = _items.Count,
                Items = items
            };
            return Task.FromResult(result);
        }

        public override Task UpdateItemAsync(IHCDataRepeaterStreamItem item)
        {
            _items[item.Id] = item;
            return Task.CompletedTask;
        }

        protected override Task StoreNewItemAsync(IHCDataRepeaterStreamItem item, object hint = null)
        {
            _items[item.Id] = item;
            return Task.CompletedTask;
        }

        public override Task<HCDataRepeaterStreamItemDetails> GetItemDetailsAsync(Guid id, string itemId)
        {
            var details = new HCDataRepeaterStreamItemDetails
            {
                Description = "Description here",
                Links = new List<HCDataRepeaterStreamItemHyperLink>
                {
                    new HCDataRepeaterStreamItemHyperLink("Test link", "/etc"),
                    new HCDataRepeaterStreamItemHyperLink("Details page", "/etc")
                }
            };
            return Task.FromResult(details);
        }

        public override Task<HCDataRepeaterItemAnalysisResult> AnalyzeItemAsync(IHCDataRepeaterStreamItem item)
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

        public override Task<HCDataRepeaterActionResult> RetryItemAsync(IHCDataRepeaterStreamItem item)
        {
            Debug.WriteLine($"Processed '{item.ItemId}'");

            var result = new HCDataRepeaterActionResult
            {
                Success = true,

                AllowRetry = false,
                Delete = false,
                RemoveAllTags = true,
                TagsThatShouldExist = new List<string> { "Processed" }
            };
            return Task.FromResult(result);
        }
    }
}
