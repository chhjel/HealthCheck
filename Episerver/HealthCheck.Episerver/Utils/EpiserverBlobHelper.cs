using EPiServer.Framework.Blobs;
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

        public EpiserverBlobHelper(IBlobFactory blobFactory,
            Func<Guid> containerIdFactory,
            Func<string> providerNameFactory)
        {
            _blobFactory = blobFactory;
            _containerIdFactory = containerIdFactory;
            _providerNameFactory = providerNameFactory;
        }

        public TData RetrieveBlobData()
        {
            var blob = CreateBlob();
            var bytes = blob.ReadAllBytes();
            if (bytes?.Any() != true)
            {
                return default;
            }

            var json = Encoding.UTF8.GetString(bytes);
            return JsonConvert.DeserializeObject<TData>(json);
        }

        public void StoreBlobData(TData data)
        {
            var blob = CreateBlob();

            var json = JsonConvert.SerializeObject(data);
            
            using var stream = blob.OpenWrite();
            var writer = new StreamWriter(stream);
            writer.WriteLine(json);
            writer.Flush();
        }

        private Uri GetContainerUri()
        {
            var containerId = _containerIdFactory.Invoke();
            var providerName = _providerNameFactory?.Invoke();
            if (providerName != null)
            {
                return Blob.GetContainerIdentifier(providerName, containerId);
            }
            return Blob.GetContainerIdentifier(containerId);
        }

        private Blob CreateBlob()
        {
            var container = GetContainerUri();
            return _blobFactory.CreateBlob(container, ".json");
        }
    }
}
