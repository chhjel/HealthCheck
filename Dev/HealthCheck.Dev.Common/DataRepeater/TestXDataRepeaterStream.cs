﻿using HealthCheck.Core.Modules.DataRepeater.Abstractions;
using HealthCheck.Core.Modules.DataRepeater.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HealthCheck.Dev.Common.DataRepeater
{
    public class TestXDataRepeaterStream : HCDataRepeaterStreamBase<TestXStreamItem>
    {
        public override string StreamDisplayName => "Test Stream";
        public override string StreamGroupName => "Test";
        public override string StreamItemsName => "Items";
        public override string ItemIdDisplayName => "Id";
        public override string RetryActionName => "Retry";
        public override string RetryDescription => null;
        public override List<string> InitiallySelectedTags => new List<string> { };
        public override List<IHCDataRepeaterStreamItemAction> Actions => new List<IHCDataRepeaterStreamItemAction>
        {
            new TestOrderDataRepeaterStreamItemActionRemoveAllTags()
        };

        public TestXDataRepeaterStream() : base(new MemoryDataRepeaterStreamItemStorage("testX"))
        {
        }

        public override Task<HCDataRepeaterStreamItemDetails> GetItemDetailsAsync(Guid id)
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

        protected override Task<HCDataRepeaterItemAnalysisResult> AnalyzeItemAsync(TestXStreamItem item)
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

        protected override Task<HCDataRepeaterRetryResult> RetryItemAsync(TestXStreamItem item)
        {
            var result = new HCDataRepeaterRetryResult
            {
                Success = true,
                Message = $"Success!",

                AllowRetry = false,
                Delete = false,
                RemoveAllTags = true,
                TagsThatShouldExist = new List<string> { "Processed" }
            };
            return Task.FromResult(result);
        }
    }
}