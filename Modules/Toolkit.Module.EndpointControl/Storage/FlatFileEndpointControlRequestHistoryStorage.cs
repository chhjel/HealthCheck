using QoDL.Toolkit.Core.Extensions;
using QoDL.Toolkit.Core.Modules.Metrics.Context;
using QoDL.Toolkit.Module.EndpointControl.Abstractions;
using QoDL.Toolkit.Module.EndpointControl.Models;
using QoDL.Toolkit.Module.EndpointControl.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace QoDL.Toolkit.Module.EndpointControl.Storage;

/// <summary>
/// Stores request data in json format on disk.
/// </summary>
public class FlatFileEndpointControlRequestHistoryStorage : IEndpointControlRequestHistoryStorage
	{
		private readonly EndpointControlRequestHistoryStorageHelper _helper = new();
		private LatestEndpointRequestsHistory _data => _helper.Data;

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
		/// The max latest number of identities to save to disk.
		/// </summary>
		public int MaxStoredIdentityCount { get; set; } = 100;

		/// <summary>
		/// Max number of latest requests to save to disk per identity.
		/// </summary>
		public int MaxStoredRequestCountPerIdentity { get; set; } = 1000;

		/// <summary>
		/// Max number of latest requests to save to disk in a separate collection only used to display latest request data in the UI.
		/// </summary>
		public int MaxStoredLatestRequestCount { get; set; } = 100;

		/// <summary>
		/// How long to delay after a new request before all data is saved to disk.
		/// </summary>
		public TimeSpan DataSaveDelay { get; set; } = TimeSpan.FromSeconds(30);

		/// <summary>
		/// For debug use, stores json prettified.
		/// </summary>
		public bool PrettyFormat { get; set; }

		private readonly object _delayedStorageLock = new();
		private readonly object _fileLock = new();
		private bool _isSaving = false;
		private readonly JsonSerializer _serializer = new();

		/// <summary>
		/// Stores request data in json format on disk.
		/// </summary>
		public FlatFileEndpointControlRequestHistoryStorage(string filePath)
    {
			FilePath = filePath;
			LoadData();
		}

		/// <summary>
		/// Deconstructor. Stores any buffered data before self destructing.
		/// </summary>
		~FlatFileEndpointControlRequestHistoryStorage()
		{
			lock (_delayedStorageLock)
			{
				if (!_isSaving)
				{
					SaveData();
				}
			}
		}

		/// <summary>
		/// Store data about a given request.
		/// </summary>
		public void AddRequest(EndpointControlEndpointRequestData request)
		{
			_helper.AddRequestToCollections(request);
			SaveDataDelayed();
		}

		/// <summary>
		/// Get the number of requests from a given location id since the given time.
		/// </summary>
		public long GetTotalRequestCountSince(string locationId, DateTimeOffset time)
		{
			lock (_data.IdentityRequests)
			{
				var queue = _data.IdentityRequests.ContainsKey(locationId) ? _data.IdentityRequests[locationId].LatestRequests : null;
				if (queue == null) { return 0; }
				lock (queue)
				{
					return queue?.Count(x => x.Timestamp >= time) ?? 0;
				}
			}
		}

		/// <summary>
		/// Get the number of requests from a given location id on a given endpoint since the given time.
		/// </summary>
		public long GetEndpointRequestCountSince(string locationId, string endpointId, DateTimeOffset time)
		{
			lock (_data.IdentityRequests)
			{
				var queue = _data.IdentityRequests.ContainsKey(locationId) ? _data.IdentityRequests[locationId].LatestRequests : null;
				if (queue == null) { return 0; }
				lock (queue)
				{
					return queue?.Count(x => x.EndpointId == endpointId && x.Timestamp >= time) ?? 0;
				}
			}
		}

		/// <summary>
		/// Get latest request history for all endpoints.
		/// </summary>
		public IEnumerable<EndpointRequestDetails> GetLatestRequests(int maxCount)
		{
			lock (_data.LatestRequests)
			{
				return _data.LatestRequests.TakeLastN(maxCount).ToArray();
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
        try
			{
				var timer = new TKMetricsTimer($"{GetType().GetFriendlyTypeName()}().SaveData()");
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
				TKMetricsContext.AddGlobalTimingValue(timer);
			}
			catch(Exception) { /* Ignored */ }
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
							LatestRequests = new Queue<EndpointRequestDetails>(x.Value.LatestRequests.Take(MaxStoredRequestCountPerIdentity)),
						};
					}),
            LatestRequestIdentities = _data.LatestRequestIdentities.Take(MaxStoredIdentityCount).ToList(),
				LatestRequests = new Queue<EndpointRequestDetails>(_data.LatestRequests.TakeLastN(MaxStoredLatestRequestCount))
        };
    }

    private void LoadData()
		{
			if (!File.Exists(FilePath))
        {
				_helper.Data = new LatestEndpointRequestsHistory();
				return;
        }

			var timer = new TKMetricsTimer($"{GetType().GetFriendlyTypeName()}().LoadData()");
			var fileReader = File.Open(FilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        using var streamReader = new StreamReader(fileReader);
        using var jsonReader = new JsonTextReader(streamReader);
			_helper.Data = _serializer.Deserialize<LatestEndpointRequestsHistory>(jsonReader);
			TKMetricsContext.AddGlobalTimingValue(timer);
		}
}
