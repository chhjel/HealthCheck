using HealthCheck.Core.Extensions;
using HealthCheck.Module.EndpointControl.Abstractions;
using HealthCheck.Module.EndpointControl.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HealthCheck.Module.EndpointControl.Storage
{
    /// <summary>
    /// Stores request data in memory only.
    /// </summary>
    public class MemoryEndpointControlRequestHistoryStorage : IEndpointControlRequestHistoryStorage
	{
		/// <summary>
		/// The max latest number of identities to store in memory.
		/// </summary>
		public int MaxIdentityCount { get; set; } = 1000;

		/// <summary>
		/// Max number of latest requests to store in memory per identity.
		/// </summary>
		public int MaxRequestCountPerIdentity { get; set; } = 1000;

		/// <summary>
		/// Max number of latest requests to store in memory.
		/// </summary>
		public int MaxMemoryLatestRequestCount { get; set; } = 500;

		private readonly LatestEndpointRequestsHistory _data = new();

		/// <inheritdoc />
		public void AddRequest(EndpointControlEndpointRequestData request)
		{
			AddRequestToCollections(request);
		}

		/// <inheritdoc />
		public long GetTotalRequestCountSince(string locationId, DateTimeOffset time)
		{
			lock (_data.IdentityRequests)
			{
				var queue = _data.IdentityRequests.ContainsKey(locationId) ? _data.IdentityRequests[locationId].LatestRequests : null;
				lock (queue)
				{
					return queue?.Count(x => x.Timestamp >= time) ?? 0;
				}
			}
		}

		/// <inheritdoc />
		public long GetEndpointRequestCountSince(string locationId, string endpointId, DateTimeOffset time)
		{
			lock (_data.IdentityRequests)
			{
				var queue = _data.IdentityRequests.ContainsKey(locationId) ? _data.IdentityRequests[locationId].LatestRequests : null;
				lock (queue)
				{
					return queue?.Count(x => x.EndpointId == endpointId && x.Timestamp >= time) ?? 0;
				}
			}
		}

		/// <inheritdoc />
		public IEnumerable<EndpointRequestDetails> GetLatestRequests(int maxCount)
		{
			lock (_data.LatestRequests)
			{
				return _data.LatestRequests.TakeLastN(maxCount).ToArray();
			}
		}

		private void AddRequestToCollections(EndpointControlEndpointRequestData request)
		{
			lock (_data.LatestRequests)
            {
				var details = CreateRequestDetails(request);
				_data.LatestRequests.Enqueue(details);

				if (_data.LatestRequests.Count > MaxMemoryLatestRequestCount)
				{
					_data.LatestRequests.Dequeue();
				}
			}

			lock (_data.LatestRequestIdentities)
			{
				// Append request if identity already exists in memory
				if (_data.IdentityRequests.ContainsKey(request.UserLocationId))
				{
					AddRequest(_data.IdentityRequests[request.UserLocationId], request);

					// Move identity to the top
					var oldIndex = _data.LatestRequestIdentities.IndexOf(request.UserLocationId);
					var oldValue = _data.LatestRequestIdentities[0];
					_data.LatestRequestIdentities[0] = _data.LatestRequestIdentities[oldIndex];
					_data.LatestRequestIdentities[oldIndex] = oldValue;
					return;
				}

				// Create new if missing
				var newItem = new LatestUserEndpointRequestHistory()
				{
					UserLocationIdentifier = request.UserLocationId
				};
				AddRequest(newItem, request);

				_data.IdentityRequests[request.UserLocationId] = newItem;
				_data.LatestRequestIdentities.Insert(0, request.UserLocationId);

				// Cleanup if needed
				if (_data.LatestRequestIdentities.Count > MaxIdentityCount)
				{
					var indexToRemove = _data.LatestRequestIdentities.Count - 1;
					var removedIdentity = _data.LatestRequestIdentities[indexToRemove];
					_data.LatestRequestIdentities.RemoveAt(indexToRemove);
					_data.IdentityRequests.Remove(removedIdentity);
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

                if (container.LatestRequests.Count > MaxRequestCountPerIdentity)
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
