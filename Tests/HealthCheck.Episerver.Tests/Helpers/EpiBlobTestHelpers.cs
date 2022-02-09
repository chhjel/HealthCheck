using EPiServer.Framework.Blobs;
using HealthCheck.Core.Abstractions;
using HealthCheck.Core.Util;
using Moq;
using System;
using System.Collections.Generic;

namespace HealthCheck.Episerver.Tests.Helpers
{
    public static class EpiBlobTestHelpers
    {
        public static List<MockBlob> CreatedBlobs { get; set; } = new List<MockBlob>();

        public static IHCCache CreateMockCache()
            => new HCSimpleMemoryCache();

        public static Mock<IBlobFactory> CreateBlobFactoryMock(Func<MockBlob> blobFactory = null, string blobJson = null)
        {
            var factory = new Mock<IBlobFactory>();
            factory
                .Setup(x => x.GetBlob(It.IsAny<Uri>()))
                .Returns<Uri>(id => blobFactory?.Invoke() ?? CreateBlob(id, blobJson));
            return factory;
        }

        public static Blob CreateBlob(Uri id, string blobJson)
            => new MockBlob(id, blobJson);
    }
}
