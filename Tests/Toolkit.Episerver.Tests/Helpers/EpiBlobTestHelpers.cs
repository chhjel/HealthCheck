using EPiServer.Events.Clients;
using EPiServer.Events.Providers.Internal;
using EPiServer.Framework.Blobs;
using QoDL.Toolkit.Core.Abstractions;
using QoDL.Toolkit.Core.Util;
using Moq;
using System;
using System.Collections.Generic;

namespace QoDL.Toolkit.Episerver.Tests.Helpers
{
    public static class EpiBlobTestHelpers
    {
        public static List<MockBlob> CreatedBlobs { get; set; } = new List<MockBlob>();

        public static ITKCache CreateMockCache()
            => new TKSimpleMemoryCache();

        public static Mock<IBlobFactory> CreateBlobFactoryMock(Func<MockBlob> blobFactory = null, string blobJson = null)
        {
            var factory = new Mock<IBlobFactory>();
            factory
                .Setup(x => x.GetBlob(It.IsAny<Uri>()))
                .Returns<Uri>(id => blobFactory?.Invoke() ?? CreateBlob(id, blobJson));
            return factory;
        }

        public static Mock<IEventRegistry> CreateEventRegistryMock()
        {
            var brokerMock = CreateEventBrokerMock().Object;
            var factory = new Mock<IEventRegistry>();
            factory
                .Setup(x => x.Get(It.IsAny<Guid>()))
                .Returns<Guid>(id => new Event(id, brokerMock, () => true));
            return factory;
        }

        public static Mock<IEventBroker> CreateEventBrokerMock()
        {
            var factory = new Mock<IEventBroker>();
            return factory;
        }

        public static Blob CreateBlob(Uri id, string blobJson)
            => new MockBlob(id, blobJson);
    }
}
