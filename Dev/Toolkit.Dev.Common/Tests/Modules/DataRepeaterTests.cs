using QoDL.Toolkit.Core.Modules.DataRepeater.Extensions;
using QoDL.Toolkit.Core.Modules.DataRepeater.Models;
using QoDL.Toolkit.Core.Modules.DataRepeater.Utils;
using QoDL.Toolkit.Core.Modules.Tests.Attributes;
using QoDL.Toolkit.Core.Modules.Tests.Models;
using QoDL.Toolkit.Core.Util;
using QoDL.Toolkit.Dev.Common.DataRepeater;
using QoDL.Toolkit.WebUI.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QoDL.Toolkit.Dev.Common.Tests.Modules;

[RuntimeTestClass(
    Name = "Data Repeater",
    DefaultRolesWithAccess = RuntimeTestAccessRole.WebAdmins,
    GroupName = RuntimeTestConstants.Group.Modules,
    UIOrder = 30
)]
public class DataRepeaterTests
{
    [RuntimeTest]
    public async Task<TestResult> StoreItem(string orderNumber, decimal amount)
    {
        var stream = new TestOrderDataRepeaterStream();
        var order = new DummyOrder
        {
            OrderNumber = orderNumber,
            Amount = amount
        };

        var item = TestOrderStreamItem.CreateFrom(order, order.OrderNumber);
        await stream.AddItemAsync(item);
        return TestResult.CreateSuccess($"Stored item!")
            .AddSerializedData(order)
            .AddSerializedData(item);
    }

    [RuntimeTest]
    public async Task<TestResult> StoreItems(int count = 368)
    {
        var stream1 = new TestOrderDataRepeaterStream();
        var stream2 = new TestXDataRepeaterStream();

        Exception dummyError = null;
        try
        {
            int.Parse("asd");
        }
        catch (Exception ex)
        {
            dummyError = ex;
        }

        for (int i = 0; i < count; i++)
        {
            var order = new DummyOrder
            {
                OrderNumber = $"X{80088888 + i}",
                Amount = 888 + i * 32.25m
            };

            TestOrderStreamItem item1;
            if (i % 2 == 0)
            {
                item1 = TestOrderStreamItem.CreateFrom(order, order.OrderNumber, $"{order.Amount}$ from \"Jimmy Smithy\"", error: "Capture failed", exception: dummyError,
                    includeTKRequestErrors: i % 3 == 0);
            }
            else
            {
                item1 = TestOrderStreamItem.CreateFrom(order, order.OrderNumber, $"{order.Amount}$ from \"Jimmy Smithy\"");
            }
            await stream1.AddItemAsync(item1);

            var item2 = TestXStreamItem.CreateFrom(new DummyX { Id = i.ToString(), Value = i + 123 }, i.ToString())
                .AddTags("SomeTag", "Another tag")
                .SetError("Hmm something happened.", dummyError);
            await stream2.AddItemAsync(item2);
        }

        return TestResult.CreateSuccess($"Stored {count} x2 items!");
    }

    [RuntimeTest]
    public TestResult AddItemThroughUtil(string orderNumber = "X8124124", decimal amount = 123m)
    {
        var order = new DummyOrder
        {
            OrderNumber = orderNumber,
            Amount = amount
        };

        var item = TestOrderStreamItem.CreateFrom(order, order.OrderNumber, $"{order.Amount}$ from \"Jimmy Smithy X\"")
            .AddTags("Tag A", "Tag B")
            .SetExpirationTime(DateTimeOffset.Now.AddDays(7));
        TKDataRepeaterUtils.AddStreamItem<TestOrderDataRepeaterStream>(item);

        return TestResult.CreateSuccess("Item was attempted added.");
    }

    [RuntimeTest]
    public async Task<TestResult> AddItemsThroughUtil(int count = 50000, string orderNumberPrefix = "X520", decimal amount = 8888m, bool serialize = false)
    {
        TKTestContext.StartTiming("Bake data");
        var items = Enumerable.Range(0, count)
            .Select(x => new DummyOrder
            {
                OrderNumber = $"{orderNumberPrefix}{x}",
                Amount = amount
            })
            .Select(order =>
            {
                return TestOrderStreamItem.CreateFrom(order, order.OrderNumber, $"{order.Amount}$ from \"Jimmy Smithy X\"", serialize: serialize)
                    .AddTags("WasBatched")
                    .SetExpirationTime(DateTimeOffset.Now.AddDays(7));
            })
            .ToArray();
        TKTestContext.EndTiming();

        TKTestContext.StartTiming("AddStreamItems");
        await TKDataRepeaterUtils.AddStreamItemsAsync<TestOrderDataRepeaterStream>(items);
        TKTestContext.EndTiming();

        return TestResult.CreateSuccess($"{count} items was attempted added.");
    }

    [RuntimeTest]
    public async Task<TestResult> SetAllowThroughExtension(string itemId, bool allowRetry)
    {
        var stream1 = new TestOrderDataRepeaterStream();
        var stream2 = new TestXDataRepeaterStream();

        var success1 = await stream1.SetAllowItemRetryAsync(itemId, allowRetry);
        var success2 = await stream2.SetAllowItemRetryAsync(itemId, allowRetry);
        var success = success1 || success2;

        return success
            ? TestResult.CreateSuccess("AllowRetry was set on item.")
            : TestResult.CreateWarning("Item not found.");
    }

    [RuntimeTest]
    public async Task<TestResult> SetAllowThroughUtility(string itemId, bool allowRetry)
    {
        var success1 = await TKDataRepeaterUtils.SetAllowItemRetryAsync<TestOrderDataRepeaterStream>(itemId, allowRetry);
        var success2 = await TKDataRepeaterUtils.SetAllowItemRetryAsync<TestXDataRepeaterStream>(itemId, allowRetry);
        var success = success1 || success2;

        return success
            ? TestResult.CreateSuccess("AllowRetry was set on item.")
            : TestResult.CreateWarning("Item not found.");
    }

    [RuntimeTest]
    public async Task<TestResult> GetItemThroughUtility(string itemId)
    {
        var item =
            await TKDataRepeaterUtils.GetItemByItemIdAsync<TestOrderDataRepeaterStream>(itemId)
            ?? await TKDataRepeaterUtils.GetItemByItemIdAsync<TestXDataRepeaterStream>(itemId);

        return item != null
            ? TestResult.CreateSuccess("Item found!").AddSerializedData(item)
            : TestResult.CreateWarning("Item not found.");
    }

    [RuntimeTest]
    public async Task<TestResult> ModifyItemsThroughUtility(List<string> itemIds, bool allowRetry = false, TKDataRepeaterStreamItemStatus status = TKDataRepeaterStreamItemStatus.Success)
    {
        var items = await TKDataRepeaterUtils.ModifyItemsAsync<TestOrderDataRepeaterStream>(itemIds, x =>
        {
            x.AllowRetry = allowRetry;
            x.ForcedStatus = status;
        });
        return TestResult.CreateSuccess("Success?").AddSerializedData(items);
    }


    [RuntimeTest]
    public TestResult Scenario1Error(string orderNumber = "X888888888", string error = "Some error here")
    {
        var order = new DummyOrder
        {
            OrderNumber = orderNumber,
            Amount = 1288.88m
        };

        var item = TestOrderStreamItem.CreateFrom(order, order.OrderNumber, "From \"John Smithery\"",
            tags: new[] { "Capture failed" },
            error: error);

        TKDataRepeaterUtils.AddStreamItem<TestOrderDataRepeaterStream>(item);
        return TestResult.CreateSuccess($"Stored item!");
    }

    [RuntimeTest]
    public TestResult Scenario1FixedInLaterAttempt(string orderNumber = "X888888888")
    {
        TKDataRepeaterUtils.SetForcedItemStatus<TestOrderDataRepeaterStream>(orderNumber, TKDataRepeaterStreamItemStatus.Success,
            new Maybe<DateTimeOffset?>(DateTimeOffset.Now.AddSeconds(30)), "Fixed!");
        TKDataRepeaterUtils.SetTags<TestOrderDataRepeaterStream>(orderNumber, new Dictionary<string, bool>() { { "Capture failed", false }, { "Capture Fixed", true } });
        return TestResult.CreateSuccess($"Marked as fixed!");
    }

    [RuntimeTest]
    public TestResult Scenario1FailedInLaterAttempt(string orderNumber = "X888888888")
    {
        TKDataRepeaterUtils.SetForcedItemStatus<TestOrderDataRepeaterStream>(orderNumber, TKDataRepeaterStreamItemStatus.Error, new Maybe<DateTimeOffset?>(null), logMessage: "Failed again!");
        TKDataRepeaterUtils.SetTags<TestOrderDataRepeaterStream>(orderNumber, new Dictionary<string, bool>() { { "Capture failed", true }, { "Capture Fixed", false } });
        return TestResult.CreateSuccess($"Marked as failed!");
    }
}
