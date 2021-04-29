﻿using EPiServer.Framework.Blobs;
using HealthCheck.Episerver.Extensions;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Text;

namespace HealthCheck.Episerver.Utils
{
    internal class EpiserverBlobHelper<TData>
    {
        private readonly IBlobFactory _blobFactory;
        private readonly Func<Guid> _containerIdFactory;
        private readonly Func<string> _providerNameFactory;
        private readonly string _blobId;

        public EpiserverBlobHelper(IBlobFactory blobFactory,
            Func<Guid> containerIdFactory,
            Func<string> providerNameFactory,
            string blobId = null)
        {
            _blobFactory = blobFactory;
            _containerIdFactory = containerIdFactory;
            _providerNameFactory = providerNameFactory;
            _blobId = blobId ?? "88888888-8888-8888-8888-888888888888.json";
        }

        public TData RetrieveBlobData()
        {
            var blob = GetBlob();
            var bytes = blob.TryReadAllBytes();
            if (bytes?.Any() != true)
            {
                return default;
            }

            var json = Encoding.UTF8.GetString(bytes);
            return JsonConvert.DeserializeObject<TData>(json);
        }

        public void StoreBlobData(TData data)
        {
            var blob = GetBlob();
            var json = JsonConvert.SerializeObject(data);
            
            using var stream = blob.OpenWrite();
            var writer = new StreamWriter(stream);
            writer.WriteLine(json);
            writer.Flush();
        }

        private Uri GetContainerId()
        {
            var containerId = _containerIdFactory.Invoke();
            var providerName = _providerNameFactory?.Invoke();
            if (providerName != null)
            {
                return Blob.GetContainerIdentifier(providerName, containerId);
            }
            return Blob.GetContainerIdentifier(containerId);
        }

        private Uri CreateBlobId()
        {
            // The unique identifier is exposed as a URI in the format:
            // epi.fx.blob://[provider]/[container]/[blob]
            var containerId = GetContainerId();
            return new Uri(containerId + $"/{_blobId}");
        }

        private Blob GetBlob()
        {
            var blobId = CreateBlobId();
            var blob = _blobFactory.GetBlob(blobId);
            return blob;
        }
    }
}
