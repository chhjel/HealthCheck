using EPiServer.Framework.Blobs;
using HealthCheck.Core.Abstractions;
using HealthCheck.Core.Util.Storage;
using HealthCheck.Episerver.Utils;
using HealthCheck.Module.EndpointControl.Abstractions;
using HealthCheck.Module.EndpointControl.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using HealthCheck.Core.Extensions;

namespace HealthCheck.Episerver.EndpointControl
{
    /// <summary>
    /// Stores request data in episerver blobstorage.
    /// </summary>
    public class HCEpiserverBlobEndpointControlRequestHistoryStorage
		: HCSingleBufferedBlobStorageBase<LatestEndpointRequestsHistory, EndpointControlEndpointRequestData>, IEndpointControlRequestHistoryStorage
	{
		/// <summary>
		/// Max duration to keep data for. Checked before saving.
		/// <para>Defaults to 7 days.</para>
		/// </summary>
		public TimeSpan MaxDataAge { get; set; } = TimeSpan.FromDays(7);

		/// <summary>
		/// The max latest number of identities to save to blob.
		/// </summary>
		public int MaxStoredIdentityCount { get; set; } = 250;

		/// <summary>
		/// Max number of latest requests to save to blob per identity.
		/// </summary>
		public int MaxStoredRequestCountPerIdentity { get; set; } = 1000;

		/// <summary>
		/// Max number of latest requests to save to blob in a separate collection only used to display latest request data in the UI.
		/// </summary>
		public int MaxStoredLatestRequestCount { get; set; } = 250;

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
			MaxBufferSize = 500;
			_blobHelper = new HCEpiserverBlobHelper<LatestEndpointRequestsHistory>(blobFactory, () => ContainerIdWithFallback, () => ProviderName);
		}

		/// <inheritdoc />
		public void AddRequest(EndpointControlEndpointRequestData request)
		{
			var data = GetBlobData();
			AddRequestToCollections(request, data);
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

		private void AddRequestToCollections(EndpointControlEndpointRequestData request, LatestEndpointRequestsHistory data)
		{
			lock (data.LatestRequests)
			{
				var details = CreateRequestDetails(request);
				data.LatestRequests.Enqueue(details);

				if (data.LatestRequests.Count > MaxStoredLatestRequestCount)
				{
					data.LatestRequests.Dequeue();
				}
			}

			lock (data.LatestRequestIdentities)
			{
				// Append request if identity already exists in memory
				if (data.IdentityRequests.ContainsKey(request.UserLocationId))
				{
					AddRequest(data.IdentityRequests[request.UserLocationId], request);

					// Move identity to the top
					var oldIndex = data.LatestRequestIdentities.IndexOf(request.UserLocationId);
					var oldValue = data.LatestRequestIdentities[0];
					data.LatestRequestIdentities[0] = data.LatestRequestIdentities[oldIndex];
					data.LatestRequestIdentities[oldIndex] = oldValue;
					return;
				}

				// Create new if missing
				var newItem = new LatestUserEndpointRequestHistory()
				{
					UserLocationIdentifier = request.UserLocationId
				};
				AddRequest(newItem, request);

				data.IdentityRequests[request.UserLocationId] = newItem;
				data.LatestRequestIdentities.Insert(0, request.UserLocationId);

				// Cleanup if needed
				if (data.LatestRequestIdentities.Count > MaxStoredIdentityCount)
				{
					var indexToRemove = data.LatestRequestIdentities.Count - 1;
					var removedIdentity = data.LatestRequestIdentities[indexToRemove];
					data.LatestRequestIdentities.RemoveAt(indexToRemove);
					data.IdentityRequests.Remove(removedIdentity);
				}
			}
		}

		private void AddRequest(LatestUserEndpointRequestHistory container, EndpointControlEndpointRequestData request)
		{
			lock (container.LatestRequests)
			{
				container.TotalRequestCount++;

				var details = CreateRequestDetails(request);
				container.LatestRequests.Enqueue(details);

				if (container.LatestRequests.Count > MaxStoredRequestCountPerIdentity)
				{
					container.LatestRequests.Dequeue();
				}
			}
		}

		private static EndpointRequestDetails CreateRequestDetails(EndpointControlEndpointRequestData request)
		{
			return new EndpointRequestDetails
			{
				UserLocationIdentifier = request.UserLocationId,
				EndpointId = request.EndpointId,
				Timestamp = request.Timestamp,
				Url = request.Url,
				UserAgent = request.UserAgent,
				WasBlocked = request.WasBlocked,
				BlockingRuleId = request.BlockingRuleId
			};
		}

	}
}
