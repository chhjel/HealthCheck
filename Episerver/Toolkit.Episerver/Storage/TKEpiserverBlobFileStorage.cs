using EPiServer.Framework.Blobs;
using QoDL.Toolkit.Core.Config;
using QoDL.Toolkit.Core.Modules.SecureFileDownload.Abstractions;
using QoDL.Toolkit.Core.Modules.SecureFileDownload.Models;
using QoDL.Toolkit.Episerver.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace QoDL.Toolkit.Episerver.Storage
{
    /// <summary>
    /// Stores files in blob storage.
    /// <para>Files will be stored in a single container (defaults to hardcoded 88888888-ff7a-40d6-a41f-cdbc654e28f1) with $"{Guid.NewGuid()}.TKUpload" as blob names.</para>
    /// </summary>
    public class TKEpiserverBlobFileStorage : ISecureFileDownloadFileStorage
    {
        /// <summary>
        /// Id of this storage.
        /// </summary>
        public string StorageId => $"EpiBlobFileStorage_{ContainerIdWithFallback}";

        /// <summary>
        /// Name of this storage.
        /// </summary>
        public string StorageName { get; set; } = "Optimizely Blob File Storage";

        /// <summary></summary>
        public string FileIdInfo => "";

        /// <summary></summary>
        public string FileIdLabel => "";

        /// <summary>
        /// Not supported.
        /// </summary>
        public bool SupportsSelectingFile => false;

        /// <summary>
        /// Is always true for this implementation, since selection is not supported.
        /// </summary>
        public bool SupportsUpload => true;

        /// <summary>
        /// Container id used if not overridden.
        /// </summary>
        protected virtual Guid DefaultContainerId => Guid.Parse("88888888-ff7a-40d6-a41f-cdbc654e28f1");

        /// <summary>
        /// Blob container to store files in. Defaults to a hardcoded guid if null
        /// </summary>
        public Guid? ContainerId { get; private set; }

        /// <summary>
        /// Shortcut to <c>ContainerId ?? DefaultContainerId</c>
        /// </summary>
        protected Guid ContainerIdWithFallback => ContainerId ?? DefaultContainerId;

        /// <summary>
        /// Defaults to the default provider if null.
        /// </summary>
        public string ProviderName { get; set; }

        private readonly IBlobFactory _blobFactory;

        /// <summary>
        /// Gets files from a folder.
        /// </summary>
        /// <param name="blobFactory">Epi blob factory.</param>
        public TKEpiserverBlobFileStorage(IBlobFactory blobFactory)
        {
            _blobFactory = blobFactory;
        }

        /// <inheritdoc />
        public Stream GetFileStream(string fileId)
        {
            var blobId = GetBlobId(fileId);
            var blob = _blobFactory.GetBlob(blobId);
            var bytes = blob.TryReadAllBytes();
            if (bytes == null) return null;

            return new MemoryStream(bytes);
        }

        /// <inheritdoc />
        public bool HasFile(string fileId)
        {
            var blobId = GetBlobId(fileId);
            var blob = _blobFactory.GetBlob(blobId);
            return blob.Exists();
        }

        /// <inheritdoc />
        public virtual string ValidateFileIdBeforeSave(string fileId) => null;

        /// <summary>
        /// Returns a list of all the files below the folder.
        /// </summary>
        public virtual IEnumerable<TKSecureFileDownloadFileDetails> GetFileIdOptions()
            => Enumerable.Empty<TKSecureFileDownloadFileDetails>();

        /// <inheritdoc />
        public Task<TKSecureFileDownloadUploadResult> UploadFileAsync(Stream fileStream)
        {
            var fileId = $"{Guid.NewGuid()}.TKUpload";
            var blobId = GetBlobId(fileId);
            var blob = _blobFactory.GetBlob(blobId);

            if (HasFile(fileId))
            {
                return Task.FromResult(new TKSecureFileDownloadUploadResult { ErrorMessage = "A file with this blob id already exists, try again." });
            }

            fileStream.Seek(0, SeekOrigin.Begin);
            blob.Write(fileStream);

            var result = new TKSecureFileDownloadUploadResult
            {
                Success = true,
                FileId = fileId
            };
            return Task.FromResult(result);
        }

        /// <inheritdoc />
        public Task<bool> DeleteUploadedFileAsync(string fileId)
        {
            if (!HasFile(fileId)) return Task.FromResult(true);

            try
            {
                var blobId = GetBlobId(fileId);
                _blobFactory.Delete(blobId);
            }
            catch (Exception ex)
            {
                TKGlobalConfig.OnExceptionEvent?.Invoke(typeof(TKEpiserverBlobFileStorage), nameof(DeleteUploadedFileAsync), ex);
                Task.FromResult(false);
            }
            return Task.FromResult(true);
        }

        private Uri GetBlobId(string fileId)
        {
            // The unique identifier is exposed as a URI in the format:
            // epi.fx.blob://[provider]/[container]/[blob]
            var containerId = GetContainerId();
            return new Uri(containerId + $"/{fileId}");
        }

        private Uri GetContainerId()
        {
            var containerId = ContainerIdWithFallback;
            if (ProviderName != null)
            {
                return Blob.GetContainerIdentifier(ProviderName, containerId);
            }
            return Blob.GetContainerIdentifier(containerId);
        }
    }
}
