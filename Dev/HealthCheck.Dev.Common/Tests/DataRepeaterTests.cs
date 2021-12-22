using HealthCheck.Core.Modules.DataRepeater.Extensions;
using HealthCheck.Core.Modules.DataRepeater.Models;
using HealthCheck.Core.Modules.DataRepeater.Utils;
using HealthCheck.Core.Modules.Tests.Attributes;
using HealthCheck.Core.Modules.Tests.Models;
using HealthCheck.Core.Util;
using HealthCheck.Dev.Common.DataRepeater;
using HealthCheck.WebUI.Extensions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HealthCheck.Dev.Common.Tests
{
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
                    OrderNumber = $"X{(80088888 + i)}",
                    Amount = (888 + (i * 32.25m))
                };

                TestOrderStreamItem item1;
                if (i % 2 == 0)
                {
                    item1 = TestOrderStreamItem.CreateFrom(order, order.OrderNumber, $"{order.Amount}$ from \"Jimmy Smithy\"", error: "Capture failed", exception: dummyError,
                        includeHCRequestErrors: (i % 3 == 0));
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
                Amount = 123m
            };

            var item = TestOrderStreamItem.CreateFrom(order, order.OrderNumber, $"{order.Amount}$ from \"Jimmy Smithy X\"")
                .AddTags("Tag A", "Tag B")
                .SetExpirationTime(DateTimeOffset.Now.AddDays(7));
            HCDataRepeaterUtils.AddStreamItem<TestOrderDataRepeaterStream>(item);

            return TestResult.CreateSuccess("Item was attempted added.");
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
            var success1 = await HCDataRepeaterUtils.SetAllowItemRetryAsync<TestOrderDataRepeaterStream>(itemId, allowRetry);
            var success2 = await HCDataRepeaterUtils.SetAllowItemRetryAsync<TestXDataRepeaterStream>(itemId, allowRetry);
            var success = success1 || success2;

            return success
                ? TestResult.CreateSuccess("AllowRetry was set on item.")
                : TestResult.CreateWarning("Item not found.");
        }

        [RuntimeTest]
        public async Task<TestResult> GetItemThroughUtility(string itemId)
        {
            var item = 
                (await HCDataRepeaterUtils.GetItemByItemIdAsync<TestOrderDataRepeaterStream>(itemId))
                ?? (await HCDataRepeaterUtils.GetItemByItemIdAsync<TestXDataRepeaterStream>(itemId));

            return item != null
                ? TestResult.CreateSuccess("Item found!").AddSerializedData(item)
                : TestResult.CreateWarning("Item not found.");
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

            HCDataRepeaterUtils.AddStreamItem<TestOrderDataRepeaterStream>(item);
            return TestResult.CreateSuccess($"Stored item!");
        }

        [RuntimeTest]
        public TestResult Scenario1FixedInLaterAttempt(string orderNumber = "X888888888")
        {
            HCDataRepeaterUtils.SetForcedItemStatus<TestOrderDataRepeaterStream>(orderNumber, HCDataRepeaterStreamItemStatus.Success,
                new Maybe<DateTimeOffset?>(DateTimeOffset.Now.AddSeconds(30)), "Fixed!");
            HCDataRepeaterUtils.SetTags<TestOrderDataRepeaterStream>(orderNumber, new Dictionary<string, bool>() { { "Capture failed", false }, { "Capture Fixed", true } });
            return TestResult.CreateSuccess($"Marked as fixed!");
        }

        [RuntimeTest]
        public TestResult Scenario1FailedInLaterAttempt(string orderNumber = "X888888888")
        {
            HCDataRepeaterUtils.SetForcedItemStatus<TestOrderDataRepeaterStream>(orderNumber, HCDataRepeaterStreamItemStatus.Error, new Maybe<DateTimeOffset?>(null), logMessage: "Failed again!");
            HCDataRepeaterUtils.SetTags<TestOrderDataRepeaterStream>(orderNumber, new Dictionary<string, bool>() { { "Capture failed", true }, { "Capture Fixed", false } });
            return TestResult.CreateSuccess($"Marked as failed!");
        }
    }
}
