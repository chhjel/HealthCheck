using QoDL.Toolkit.Core.Modules.EventNotifications.Models;
using System;
using Xunit;
using Xunit.Abstractions;
using static QoDL.Toolkit.Core.Tests.Modules.EventNotifications.EventNotificationsTestHelpers;

namespace QoDL.Toolkit.Core.Tests.Modules.EventNotifications;

public class RegisterEventTests
{
    public ITestOutputHelper Output { get; }

    public RegisterEventTests(ITestOutputHelper output)
    {
        Output = output;
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(10)]
    [InlineData(50)]
    public void RegisterEvent_NotifiesEqualToCount(int eventCount)
    {
        var sink = CreateEventDataSink(out TestEventNotifier testEventNotifier, out EventSinkNotificationConfig testConfig);

        for (int i = 0; i < eventCount; i++)
        {
            sink.RegisterEvent("test_event", fireAndForget: false);
        }

        Output.WriteLine($"Notified {testEventNotifier.NotifiedEvents.Count} test events.");
        Assert.Equal(eventCount, testEventNotifier.NotifiedEvents.Count);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(10)]
    public void RegisterEvent_DistinctNotification_StaticKey_NotifiesSingle(int eventCount)
    {
        var sink = CreateEventDataSink(out TestEventNotifier testEventNotifier, out EventSinkNotificationConfig testConfig);
        testConfig.DistinctNotificationKey = "static_key";
        testConfig.DistinctNotificationCacheDuration = TimeSpan.FromMinutes(5);

        for (int i=0; i < eventCount; i++)
        {
            sink.RegisterEvent("test_event", fireAndForget: false);
        }

        Output.WriteLine($"Notified {testEventNotifier.NotifiedEvents.Count} test events.");
        Assert.Single(testEventNotifier.NotifiedEvents);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(10)]
    public void RegisterEvent_DistinctNotification_DynamicKeysAllUnique_NotifiesAll(int eventCount)
    {
        var sink = CreateEventDataSink(out TestEventNotifier testEventNotifier, out EventSinkNotificationConfig testConfig);
        testConfig.DistinctNotificationKey = "dynamic_key_{ORDERNUMBER}";
        testConfig.DistinctNotificationCacheDuration = TimeSpan.FromMinutes(5);

        for (int i = 0; i < eventCount; i++)
        {
            var payload = new
            {
                OrderNumber = (i + 10000).ToString()
            };

            sink.RegisterEvent("test_event", payload, fireAndForget: false);
        }

        Output.WriteLine($"Notified {testEventNotifier.NotifiedEvents.Count} test events.");
        Assert.Equal(eventCount, testEventNotifier.NotifiedEvents.Count);
    }

    [Fact]
    public void RegisterEvent_DistinctNotification_DynamicKeys2Unique_Notifies2()
    {
        var sink = CreateEventDataSink(out TestEventNotifier testEventNotifier, out EventSinkNotificationConfig testConfig);
        testConfig.DistinctNotificationKey = "dynamic_key_{ISEVEN}";
        testConfig.DistinctNotificationCacheDuration = TimeSpan.FromMinutes(5);

        for (int i = 1; i <= 10; i++)
        {
            var payload = new
            {
                IsEven = (i % 2 == 0).ToString()
            };

            Output.WriteLine($"Payload #{i}: IsEven={payload.IsEven}");
            sink.RegisterEvent("test_event", payload, fireAndForget: false);
        }

        Output.WriteLine($"Notified {testEventNotifier.NotifiedEvents.Count} test events.");
        Assert.Equal(2, testEventNotifier.NotifiedEvents.Count);
    }
}
