using QoDL.Toolkit.Core.Modules.DataRepeater.Abstractions;
using QoDL.Toolkit.Core.Modules.DataRepeater.Models;
using QoDL.Toolkit.Core.Modules.DataRepeater.Storage;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace QoDL.Toolkit.Dev.Common.DataRepeater;

public class TestXDataRepeaterStream : TKDataRepeaterStreamBase<TestXStreamItem>
{
    public override string StreamDisplayName => "Test Stream";
    public override string StreamGroupName => "Test";
    public override string StreamItemsName => "Items";
    public override string ItemIdDisplayName => "Id";
    public override string RetryActionName => "Retry";
    public override string RetryDescription => null;
    public override bool ManualAnalyzeEnabled => false;
    public override List<string> InitiallySelectedTags => new() { };
    public override List<string> FilterableTags => new() { };
    public override object AllowedAccessRoles => RuntimeTestAccessRole.SystemAdmins;
    public override List<string> Categories => new() { "TestX" };

    public override List<ITKDataRepeaterStreamItemAction> Actions => new()
    {
        new TestOrderDataRepeaterStreamItemActionRemoveAllTags()
    };
    public override List<ITKDataRepeaterStreamItemBatchAction> BatchActions => new();

    public TestXDataRepeaterStream() : base(new MemoryDataRepeaterStreamItemStorage("testX"))
    {
    }

    protected override Task<TKDataRepeaterStreamItemDetails> GetItemDetailsAsync(TestXStreamItem item)
    {
        var details = new TKDataRepeaterStreamItemDetails
        {
            DescriptionHtml = "Description here",
            Links = new List<TKDataRepeaterStreamItemHyperLink>
            {
                new TKDataRepeaterStreamItemHyperLink("Test link", "/etc"),
                new TKDataRepeaterStreamItemHyperLink("Details page", "/etc")
            }
        };
        return Task.FromResult(details);
    }

    protected override Task<TKDataRepeaterItemAnalysisResult> AnalyzeItemAsync(TestXStreamItem item, bool isManualAnalysis = false)
    {
        var result = new TKDataRepeaterItemAnalysisResult();

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

    protected override Task<TKDataRepeaterRetryResult> RetryItemAsync(TestXStreamItem item)
    {
        var result = new TKDataRepeaterRetryResult
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
