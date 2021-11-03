using HealthCheck.Episerver.EndpointControl;
using HealthCheck.Episerver.Tests.Helpers;
using HealthCheck.Module.EndpointControl.Models;
using System;
using System.Linq;
using Xunit;

namespace HealthCheck.Episerver.Tests.Storage
{
    public class HCEpiserverBlobEndpointControlRequestHistoryStorageTests
    {
        [Fact]
        public void AddRequest_WithManyUniqueAdditions_ShouldNotStoreMoreThanNeeded()
        {
            var blob = new MockBlob(new Uri("https://mock.blob"), "{}");
            var storage = CreateStorage(() => blob);
            storage.MaxStoredLatestRequestCount = 250;
            storage.MaxStoredIdentityCount = 250;
            storage.MaxStoredRequestCountPerIdentity = 1000;
            storage.MaxBufferSize = 2500;
            
            for (int i = 0; i < 50000; i++)
            {
                var request = new EndpointControlEndpointRequestData
                {
                    ActionName = $"ActionName#{i}",
                    BlockingRuleId = Guid.NewGuid(),
                    ControllerName = $"#ControllerName{i}",
                    ControllerType = GetType(),
                    EndpointId = $"EndpointId#{i}",
                    EndpointName = $"EndpointName#{i}",
                    HttpMethod = "GET",
                    Timestamp = DateTimeOffset.Now,
                    Url = $"Url#{i}",
                    UserAgent = $"UserAgent#{i}",
                    UserLocationId = $"UserLocationId#{i}",
                    WasBlocked = i % 2 == 0
                };
                storage.AddRequest(request);
            }

            var latestRequest = storage.GetLatestRequests(int.MaxValue);
            Assert.Equal(storage.MaxStoredLatestRequestCount, latestRequest.Count());
            Assert.Equal(1, storage.LoadCounter); // Should get only once
            Assert.Equal(20, storage.SaveCounter); // Store count should be inserts / max buffer size = 0
        }

        [Fact]
        public void AddRequest_WithManyAdditionsPerLocation_ShouldNotStoreMoreThanNeeded()
        {
            var blob = new MockBlob(new Uri("https://mock.blob"), "{}");
            var storage = CreateStorage(() => blob);
            storage.MaxStoredLatestRequestCount = 250;
            storage.MaxStoredIdentityCount = 250;
            storage.MaxStoredRequestCountPerIdentity = 1000;
            storage.MaxBufferSize = 2500;

            for (int identityNumber = 0; identityNumber < 100; identityNumber++)
            {
                for (int i = 0; i < 100; i++)
                {
                    var request = new EndpointControlEndpointRequestData
                    {
                        ActionName = $"ActionName#{i}",
                        BlockingRuleId = Guid.NewGuid(),
                        ControllerName = $"#ControllerName{i}",
                        ControllerType = GetType(),
                        EndpointId = $"EndpointId#{i}",
                        EndpointName = $"EndpointName#{i}",
                        HttpMethod = "GET",
                        Timestamp = DateTimeOffset.Now,
                        Url = $"Url#{i}",
                        UserAgent = $"UserAgent#{i}",
                        UserLocationId = $"UserLocationId#{identityNumber}",
                        WasBlocked = i % 2 == 0
                    };
                    storage.AddRequest(request);
                }
            }

            var latestRequest = storage.GetLatestRequests(int.MaxValue);
            Assert.Equal(storage.MaxStoredLatestRequestCount, latestRequest.Count());
            Assert.Equal(1, storage.LoadCounter); // Should get only once
            Assert.Equal(4, storage.SaveCounter); // Store count should be inserts / max buffer size = 0
        }

        private HCEpiserverBlobEndpointControlRequestHistoryStorage CreateStorage(Func<MockBlob> blobFactory = null, string blobJson = null)
        {
            var cache = EpiBlobTestHelpers.CreateMockCache();
            var factoryMock = EpiBlobTestHelpers.CreateBlobFactoryMock(blobFactory, blobJson);
            return new HCEpiserverBlobEndpointControlRequestHistoryStorage(factoryMock.Object, cache);
        }
    }
}
