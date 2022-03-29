using EPiServer.Framework.Blobs;
using HealthCheck.Core.Abstractions;
using HealthCheck.Core.Extensions;
using HealthCheck.Core.Util.Storage;
using HealthCheck.Episerver.Utils;
using HealthCheck.Module.EndpointControl.Abstractions;
using HealthCheck.Module.EndpointControl.Models;
using HealthCheck.Module.EndpointControl.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HealthCheck.Episerver.EndpointControl
{
    /// <summary>
    /// Stores request data in episerver blobstorage.
    /// </summary>
    public class HCEpiserverBlobEndpointControlRequestHistoryStorage
		: HCSingleBufferedBlobStorageBase<LatestEndpointRequestsHistory, EndpointControlEndpointRequestData>, IEndpointControlRequestHistoryStorage
	{
		private readonly EndpointControlRequestHistoryStorageHelper _helper = new()
		{
			MaxMemoryIdentityCount = 250,
			MaxMemoryRequestCountPerIdentity = 1000,
			MaxMemoryLatestRequestCount = 250
		};

		/// <summary>
		/// Max duration to keep data for. Checked before saving.
		/// <para>Defaults to 7 days.</para>
		/// </summary>
		public TimeSpan MaxDataAge { get; set; } = TimeSpan.FromDays(7);

		/// <summary>
		/// The max latest number of identities to save to blob.
		/// </summary>
		public int MaxStoredIdentityCount { get => _helper.MaxMemoryIdentityCount; set => _helper.MaxMemoryIdentityCount = value; }

		/// <summary>
		/// Max number of latest requests to save to blob per identity.
		/// </summary>
		public int MaxStoredRequestCountPerIdentity { get => _helper.MaxMemoryRequestCountPerIdentity; set => _helper.MaxMemoryRequestCountPerIdentity = value; }

		/// <summary>
		/// Max number of latest requests to save to blob in a separate collection only used to display latest request data in the UI.
		/// </summary>
		public int MaxStoredLatestRequestCount { get => _helper.MaxMemoryLatestRequestCount; set => _helper.MaxMemoryLatestRequestCount = value; }

		/// <summary>
		/// Container id used if not overridden.
		/// </summary>
		protected virtual Guid DefaultContainerId => Guid.Parse("836920d7-41b5-47d0-9bfa-858a2018f473");

		/// <summary>
		/// Defaults to the default provider if null.
		/// </summary>
		public string ProviderName { get; set; }

		/// <summary>
		/// Defaults to a hardcoded guid if null
		/// </summary>
		public Guid? ContainerId { get; set; }

		/// <summary>
		/// Shortcut to <c>ContainerId ?? DefaultContainerId</c>
		/// </summary>
		protected Guid ContainerIdWithFallback => ContainerId ?? DefaultContainerId;

		/// <inheritdoc />
		protected override string CacheKey => $"__hc_{ContainerIdWithFallback}";

		private readonly HCEpiserverBlobHelper<LatestEndpointRequestsHistory> _blobHelper;

		/// <summary>
		/// Stores request data in episerver blobstorage.
		/// </summary>
		public HCEpiserverBlobEndpointControlRequestHistoryStorage(IBlobFactory blobFactory, IHCCache cache)
			: base(cache)
		{
			BlobUpdateBufferDuration = TimeSpan.FromSeconds(30);
			MaxBufferSize = 2500;
			_blobHelper = new HCEpiserverBlobHelper<LatestEndpointRequestsHistory>(blobFactory, () => ContainerIdWithFallback, () => ProviderName);
		}

		/// <inheritdoc />
		public void AddRequest(EndpointControlEndpointRequestData request)
		{
			var data = GetBlobData();
			_helper.AddRequestToCollections(request, data);
			InsertItemBuffered(request);
		}

		/// <inheritdoc />
		public long GetTotalRequestCountSince(string locationId, DateTimeOffset time)
		{
			var data = GetBlobData();
			lock (data.IdentityRequests)
			{
				var queue = data.IdentityRequests.ContainsKey(locationId) ? data.IdentityRequests[locationId].LatestRequests : null;
				if (queue == null) { return 0; }
				lock (queue)
				{
					return queue?.Count(x => x.Timestamp >= time) ?? 0;
				}
			}
		}

		/// <inheritdoc />
		public long GetEndpointRequestCountSince(string locationId, string endpointId, DateTimeOffset time)
		{
			var data = GetBlobData();
			lock (data.IdentityRequests)
			{
				var queue = data.IdentityRequests.ContainsKey(locationId) ? data.IdentityRequests[locationId].LatestRequests : null;
				if (queue == null) { return 0; }
				lock (queue)
				{
					return queue?.Count(x => x.EndpointId == endpointId && x.Timestamp >= time) ?? 0;
				}
			}
		}

		/// <inheritdoc />
		public IEnumerable<EndpointRequestDetails> GetLatestRequests(int maxCount)
			=> GetBlobData().LatestRequests.TakeLastN(maxCount).ToArray();

		/// <inheritdoc />
		protected override LatestEndpointRequestsHistory UpdateDataFromBuffer(LatestEndpointRequestsHistory data, Queue<BufferQueueItem> bufferedItems)
		{
			// Adding instantly in insert and using buffer for delayed save
			// foreach(var request in bufferedItems)
			//	AddRequestToCollections(request.Item, data)

			CleanupOldData(data);
			return data;
		}

		/// <inheritdoc />
		protected override LatestEndpointRequestsHistory RetrieveBlobData() => _blobHelper.RetrieveBlobData();

		/// <inheritdoc />
		protected override void StoreBlobData(LatestEndpointRequestsHistory data) => _blobHelper.StoreBlobData(data);

		private void CleanupOldData(LatestEndpointRequestsHistory data)
		{
			var dateThreshold = DateTimeOffset.Now - MaxDataAge;
			var identitiesToRemove = new HashSet<string>();

			// Remove requests
			foreach (var identityKvp in data.IdentityRequests)
			{
				identityKvp.Value.LatestRequests = new Queue<EndpointRequestDetails>(
					identityKvp.Value.LatestRequests.Where(x => x.Timestamp >= dateThreshold).ToArray()
				);

				if (identityKvp.Value.LatestRequests.Count == 0)
				{
					identitiesToRemove.Add(identityKvp.Key);
				}
			}

			// Remove identities without any more requests
			foreach (var identity in identitiesToRemove)
			{
				data.LatestRequestIdentities.Remove(identity);
				data.IdentityRequests.Remove(identity);
			}
		}
	}
}
