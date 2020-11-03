#if NETFULL
using HealthCheck.Module.EndpointControl.Abstractions;
using HealthCheck.Module.EndpointControl.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace HealthCheck.Module.EndpointControl.Storage
{
    /// <summary>
    /// Stores request data in json format on disk.
    /// </summary>
    public class FlatFileEndpointControlRequestHistoryStorage : IEndpointControlRequestHistoryStorage
	{
		/// <summary>
		/// Filepath where the flatfile will be stored.
		/// </summary>
		public string FilePath { get; set; }

		/// <summary>
		/// Max duration to keep data for. Checked before saving.
		/// <para>Defaults to 7 days.</para>
		/// </summary>
		public TimeSpan MaxDataAge { get; set; } = TimeSpan.FromDays(7);

		/// <summary>
		/// The max latest number of identities to store in memory.
		/// </summary>
		public int MaxMemoryIdentityCount { get; set; } = 1000;

		/// <summary>
		/// The max latest number of identities to save to disk.
		/// </summary>
		public int MaxStoredIdentityCount { get; set; } = 100;

		/// <summary>
		/// Max number of latest requests to store in memory per identity.
		/// </summary>
		public int MaxMemoryRequestCountPerIdentity { get; set; } = 1000;

		/// <summary>
		/// Max number of latest requests to save to disk per identity.
		/// </summary>
		public int MaxStoredRequestCountPerIdentity { get; set; } = 1000;

		/// <summary>
		/// How long to delay after a new request before all data is saved to disk.
		/// </summary>
		public TimeSpan DataSaveDelay { get; set; } = TimeSpan.FromSeconds(30);

		/// <summary>
		/// For debug use, stores json prettified.
		/// </summary>
		public bool PrettyFormat { get; set; }

		private LatestEndpointRequestsHistory _data = new LatestEndpointRequestsHistory();
		private readonly object _delayedStorageLock = new object();
		private readonly object _fileLock = new object();
		private bool _isSaving = false;
		private readonly JsonSerializer _serializer = new JsonSerializer();

		/// <summary>
		/// Stores request data in json format on disk.
		/// </summary>
		public FlatFileEndpointControlRequestHistoryStorage(string filePath)
        {
			FilePath = filePath;
			LoadData();
        }

		/// <summary>
		/// Store data about a given request.
		/// </summary>
		public void AddRequest(EndpointControlEndpointRequestData request)
		{
			AddRequestToCollections(request);
			SaveDataDelayed();
		}

		/// <summary>
		/// Get the number of requests from a given location id since the given time.
		/// </summary>
		public long GetTotalRequestCountSince(string locationId, DateTimeOffset time)
		{
			lock (_data.IdentityRequests)
			{
				return _data.IdentityRequests.ContainsKey(locationId)
					? _data.IdentityRequests[locationId].LatestRequests.Count(x => x.Timestamp >= time)
					: 0;
			}
		}

		/// <summary>
		/// Get the number of requests from a given location id on a given endpoint since the given time.
		/// </summary>
		public long GetEndpointRequestCountSince(string locationId, string endpointId, DateTimeOffset time)
		{
			lock (_data.IdentityRequests)
			{
				return _data.IdentityRequests.ContainsKey(locationId)
					? _data.IdentityRequests[locationId].LatestRequests.Count(x => x.EndpointId == endpointId && x.Timestamp >= time)
					: 0;
			}
		}

		private void SaveDataDelayed()
		{
			lock (_delayedStorageLock)
			{
				if (!_isSaving)
				{
					_isSaving = true;
					Task.Run(async () =>
					{
						await Task.Delay(DataSaveDelay);
						SaveData();
						_isSaving = false;
					});
				}
			}
		}

		private void SaveData()
		{
			LatestEndpointRequestsHistory dataCopy = null;

			lock (_data.LatestRequestIdentities)
			{
				lock (_data.IdentityRequests)
                {
                    CleanupOldData();

                    dataCopy = CreateDataCopyForStorage();
                }
            }

			var json = JsonConvert.SerializeObject(dataCopy, PrettyFormat ? Formatting.Indented : Formatting.None);
			lock (_fileLock)
			{
				File.WriteAllText(FilePath, json);
			}
		}

		private void CleanupOldData()
		{
			var dateThreshold = DateTimeOffset.Now - MaxDataAge;
			var identitiesToRemove = new HashSet<string>();

			// Remove requests
			foreach (var identityKvp in _data.IdentityRequests)
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
				_data.LatestRequestIdentities.Remove(identity);
				_data.IdentityRequests.Remove(identity);
			}
		}

		private LatestEndpointRequestsHistory CreateDataCopyForStorage()
        {
            return new LatestEndpointRequestsHistory()
            {
                IdentityRequests = _data.IdentityRequests
					.Take(MaxStoredIdentityCount)
					.ToDictionary(x => x.Key, x =>
					{
						return new LatestUserEndpointRequestHistory
						{
							UserLocationIdentifier = x.Value.UserLocationIdentifier,
							TotalRequestCount = x.Value.TotalRequestCount,
							LatestRequests = new Queue<EndpointRequestDetails>(x.Value.LatestRequests.Take(MaxStoredRequestCountPerIdentity))
						};
					}),
                LatestRequestIdentities = _data.LatestRequestIdentities.Take(MaxStoredIdentityCount).ToList(),
            };
        }

        private void LoadData()
		{
			if (!File.Exists(FilePath))
            {
				_data = new LatestEndpointRequestsHistory();
				return;
            }

			var fileReader = File.Open(FilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using var streamReader = new StreamReader(fileReader);
            using var jsonReader = new JsonTextReader(streamReader);
            _data = _serializer.Deserialize<LatestEndpointRequestsHistory>(jsonReader);
        }

		private void AddRequestToCollections(EndpointControlEndpointRequestData request)
		{
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
				if (_data.LatestRequestIdentities.Count > MaxMemoryIdentityCount)
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

				var details = new EndpointRequestDetails
				{
					UserLocationIdentifier = request.UserLocationId,
					EndpointId = request.EndpointId,
					Timestamp = request.Timestamp,
					Url = request.Url,
					UserAgent = request.UserAgent,
					WasBlocked = request.WasBlocked,
					BlockingRuleId = request.BlockingRuleId
				};
				container.LatestRequests.Enqueue(details);

				if (container.LatestRequests.Count > MaxMemoryRequestCountPerIdentity)
				{
					container.LatestRequests.Dequeue();
				}
			}
		}
	}
}
#endif
