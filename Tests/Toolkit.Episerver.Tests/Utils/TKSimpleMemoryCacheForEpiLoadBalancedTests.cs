using EPiServer.Events.Clients;
using Moq;
using QoDL.Toolkit.Episerver.Tests.Helpers;
using QoDL.Toolkit.Episerver.Utils;
using System;
using System.Collections.Generic;
using Xunit;

namespace QoDL.Toolkit.Episerver.Tests.Utils
{
    public class TKSimpleMemoryCacheForEpiLoadBalancedTests
    {
        [Fact]
        public void SetValue_WithoutDistribution_DoesNotCreateEvent()
        {
            var cache = CreateCache(out var _);
            cache.SetValue("keyA", "ValueA", TimeSpan.FromMinutes(1));
            Assert.False(cache.HasRaisedAnyEvents);
        }

        [Fact]
        public void ClearAll_WithoutDistribution_CreatesEvent()
        {
            var cache = CreateCache(out var _);
            cache.ClearAll();
            Assert.False(cache.HasRaisedAnyEvents);
        }

        [Fact]
        public void ClearKey_WithoutDistribution_CreatesEvent()
        {
            var cache = CreateCache(out var _);
            cache.ClearKey("keyA");
            Assert.False(cache.HasRaisedAnyEvents);
        }

        [Fact]
        public void SetValue_WithDistribution_DoesNotCreateEvent()
        {
            var cache = CreateCache(out var _);
            cache.SetValue("keyA", "ValueA", TimeSpan.FromMinutes(1), true);
            Assert.True(cache.HasRaisedAnyEvents);
        }

        [Fact]
        public void ClearAll_WithDistribution_CreatesEvent()
        {
            var cache = CreateCache(out var _);
            cache.ClearAll(true);
            Assert.True(cache.HasRaisedAnyEvents);
        }

        [Fact]
        public void ClearKey_WithDistribution_CreatesEvent()
        {
            var cache = CreateCache(out var _);
            cache.ClearKey("keyA", true);
            Assert.True(cache.HasRaisedAnyEvents);
        }

        [Fact]
        public void Set_WithMultipleInstances_InvalidatesOnOthers()
        {
            var instances = new SimulatedInstances();
            var instanceA = instances.AddInstance();
            instanceA.Cache.Set("KeyA", "ValueA", TimeSpan.FromHours(1), allowDistribute: false);
            var instanceB = instances.AddInstance();
            instanceB.Cache.Set("KeyA", "ValueA", TimeSpan.FromHours(1), allowDistribute: false);
            var instanceC = instances.AddInstance();
            instanceC.Cache.Set("KeyA", "ValueA", TimeSpan.FromHours(1), allowDistribute: false);

            Assert.False(instanceA.Cache.HasRaisedAnyEvents);
            Assert.False(instanceB.Cache.HasRaisedAnyEvents);
            Assert.False(instanceC.Cache.HasRaisedAnyEvents);
            Assert.False(instanceA.Cache.HasReceivedAnyEvents);
            Assert.False(instanceB.Cache.HasReceivedAnyEvents);
            Assert.False(instanceC.Cache.HasReceivedAnyEvents);

            instanceA.Cache.Set("KeyA", "ValueB", TimeSpan.FromHours(1), allowDistribute: true);

            Assert.True(instanceA.Cache.HasRaisedAnyEvents);
            Assert.False(instanceB.Cache.HasRaisedAnyEvents);
            Assert.False(instanceC.Cache.HasRaisedAnyEvents);
            Assert.False(instanceA.Cache.HasReceivedAnyEvents);
            Assert.True(instanceB.Cache.HasReceivedAnyEvents);
            Assert.True(instanceC.Cache.HasReceivedAnyEvents);

            Assert.Equal("ValueB", instanceA.Cache.GetValue<string>("KeyA"));
            Assert.False(instanceB.Cache.ContainsKey("KeyA"));
            Assert.False(instanceC.Cache.ContainsKey("KeyA"));
        }

        private static TKSimpleMemoryCacheForEpiLoadBalanced CreateCache(out Mock<IEventRegistry> eventRegistryMock)
        {
            eventRegistryMock = EpiBlobTestHelpers.CreateEventRegistryMock();
            return new TKSimpleMemoryCacheForEpiLoadBalanced(eventRegistryMock.Object);
        }

        private class SimulatedInstance
        {
            public TKSimpleMemoryCacheForEpiLoadBalanced Cache { get; private set; }
            public SimulatedInstance()
            {
                Cache = CreateCache(out var _);
            }
        }

        private class SimulatedInstances
        {
            public List<SimulatedInstance> Instances { get; private set; } = new List<SimulatedInstance>();

            public SimulatedInstance AddInstance()
            {
                var instance = new SimulatedInstance();
                var raiserId = Guid.NewGuid();
                instance.Cache._raiserIdOverride = raiserId;
                Instances.Add(instance);
                instance.Cache.OnEventRaised += (eventId, message) => OnEventRaised(instance, raiserId, eventId, message);
                return instance;
            }

            private void OnEventRaised(SimulatedInstance instance, Guid raiserId, Guid eventId, string message)
            {
                var args = new EPiServer.Events.EventNotificationEventArgs(raiserId, eventId, message);
                Instances.ForEach(x => x.Cache.InvalidationEvent_Raised(null, args));
            }
        }
    }
}
