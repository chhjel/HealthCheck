using Microsoft.Extensions.Caching.Memory;
using System.Collections.Generic;

namespace HealthCheck.Utility.Storage.Tests.Implementations
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Major Code Smell", "S3881:\"IDisposable\" should be implemented correctly", Justification = "Test")]
	public class TestCache : IMemoryCache
	{
		public int CreateEntryCounter { get; set; }
		public int RemoveEntryCounter { get; set; }
		public int CacheUsedCounter { get; set; }
		public int CacheNotUsedCounter { get; set; }

		private readonly Dictionary<object, ICacheEntry> _cache = new();

		public ICacheEntry CreateEntry(object key)
		{
			CreateEntryCounter++;

			var item = new TestCacheEntry
			{
				Key = key
			};
			_cache[key] = item;
			return item;
		}

		public void Dispose() { /*Ignored*/ }

		public void Remove(object key)
		{
			RemoveEntryCounter++;
			_cache.Remove(key);
		}

		public bool TryGetValue(object key, out object value)
		{
			if (_cache.ContainsKey(key))
			{
				value = _cache[key].Value;
				CacheUsedCounter++;
				return true;
			}

			value = default;
			CacheNotUsedCounter++;
			return false;
		}
	}
}
