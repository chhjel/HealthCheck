using HealthCheck.Core.Abstractions;
using HealthCheck.Core.Extensions;
using HealthCheck.Core.Modules.Metrics.Context;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace HealthCheck.Core.Util
{
    /// <summary>
    /// Stores items serialized on disk and keeps them in memory for fast access.
	/// <para>Optimized for few items, somewhere less than 500.</para>
	/// <para>Should be used as a singleton.</para>
    /// </summary>
    public class HCSimpleCachedDataStore<TData>
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
		/// The max latest number of items to store in memory.
		/// <para>Defaults to 200.</para>
		/// </summary>
		public int MaxMemoryItemCount { get; set; } = 200;

		/// <summary>
		/// The max latest number of items to store in memory.
		/// <para>Defaults to 100.</para>
		/// </summary>
		public int MaxStoredItemCount { get; set; } = 100;

		/// <summary>
		/// How long to delay after a new item is added before all data is saved to disk.
		/// </summary>
		public TimeSpan DataSaveDelay { get; set; } = TimeSpan.FromSeconds(5);

		private SimpleCachedDataContainer _container = new();

		private readonly object _delayedStorageLock = new();
		private readonly object _fileLock = new();
		private bool _isSaving = false;
		private readonly IJsonSerializer _serializer;

		/// <summary>
		/// Stores items serialized on disk and keeps them in memory for fast access.
		/// </summary>
		public HCSimpleCachedDataStore(IJsonSerializer serializer, string filePath)
		{
			_serializer = serializer;
			FilePath = filePath;
			LoadData();
		}

		/// <summary>
		/// Deconstructor. Stores any buffered data before self destructing.
		/// </summary>
		~HCSimpleCachedDataStore()
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
		/// Store a new item.
		/// </summary>
		public void AddItem(TData data)
		{
			AddItemInternal(data);
			SaveDataDelayed();
		}

		/// <summary>
		/// Removes all matching items.
		/// </summary>
		public void RemoveAll(Func<TData, bool> predicate)
		{
			RemoveAllInternal(predicate);
			SaveDataDelayed();
		}

		/// <summary>
		/// Removes all data.
		/// </summary>
		public void Clear()
		{
			lock (_container)
			{
				_container.Items.Clear();
				_container.TotalCount = 0;
				SaveDataDelayed();
			}
		}

		/// <summary>
		/// Get all items.
		/// </summary>
		public IEnumerable<TData> GetItems()
		{
			lock (_container)
			{
				return _container.Items.Select(x => x.Data);
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
				SimpleCachedDataContainer dataCopy = null;

				lock (_container)
				{
					CleanupOldData();

					dataCopy = CreateDataCopyForStorage();
				}

				var json = _serializer.Serialize(dataCopy);
				lock (_fileLock)
				{
					var timer = new HCMetricsTimer($"{GetType().GetFriendlyTypeName()}.SaveData()");
					var folder = Directory.GetParent(FilePath).FullName;
					if (!Directory.Exists(folder))
                    {
						Directory.CreateDirectory(folder);
                    }
					File.WriteAllText(FilePath, json);
					HCMetricsContext.AddGlobalTimingValue(timer);
				}
			}
			catch (Exception) {/* Ignored */ }
		}

		private void CleanupOldData()
		{
			var dateThreshold = DateTimeOffset.Now - MaxDataAge;

			_container.Items.RemoveAll(x => x.AddedAt < dateThreshold);
			_container.Items = _container.Items.Take(MaxMemoryItemCount).ToList();
			_container.TotalCount = _container.Items.Count;
		}

		private SimpleCachedDataContainer CreateDataCopyForStorage()
		{
			var itemsToStore = _container.Items
				.Take(MaxStoredItemCount)
				.ToList();

			return new SimpleCachedDataContainer()
			{
				TotalCount = itemsToStore.Count,
				Items = itemsToStore
			};
		}

		private void LoadData()
		{
			if (!File.Exists(FilePath))
			{
				_container = new SimpleCachedDataContainer();
				return;
			}

			var timer = new HCMetricsTimer($"{GetType().GetFriendlyTypeName()}.LoadData()");
			var contents = HCIOUtils.ReadFile(FilePath);
			_container = _serializer.Deserialize<SimpleCachedDataContainer>(contents)
				?? new SimpleCachedDataContainer();
			HCMetricsContext.AddGlobalTimingValue(timer);
		}

		private void AddItemInternal(TData data)
		{
			lock (_container)
			{
				_container.Items.Insert(0, new SimpleCachedDataItemWrapper
				{
					AddedAt = DateTimeOffset.Now,
					Data = data
				});
				_container.TotalCount++;

				if (_container.Items.Count > MaxMemoryItemCount)
				{
					_container.Items.RemoveAt(MaxMemoryItemCount - 1);
					_container.TotalCount--;
				}
			}
		}

		private void RemoveAllInternal(Func<TData, bool> predicate)
		{
			lock (_container)
			{
				_container.Items = _container.Items.Where(x => !predicate(x.Data)).ToList();
				_container.TotalCount++;
			}
		}

		#region Helper classes
		private class SimpleCachedDataContainer
		{
			public int TotalCount { get; set; }
			public List<SimpleCachedDataItemWrapper> Items { get; set; } = new List<SimpleCachedDataItemWrapper>();
		}

		private struct SimpleCachedDataItemWrapper
		{
			public DateTimeOffset AddedAt { get; set; }
			public TData Data { get; set; }
		}
		#endregion
	}
}
