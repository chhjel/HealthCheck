using HealthCheck.Core.Util;
using System;

namespace HealthCheck.Core.Tests.Storage.Implementations
{
    public class TestCache : SimpleMemoryCache
	{
		public int CreateEntryCounter { get; set; }
		public int RemoveEntryCounter { get; set; }
		public int CacheUsedCounter { get; set; }
		public int CacheNotUsedCounter { get; set; }

        public override T Set<T>(string key, T value, TimeSpan absoluteExpirationRelativeToNow)
        {
			CreateEntryCounter++;
			return base.Set(key, value, absoluteExpirationRelativeToNow);
        }

        public override bool TryGetValue<T>(string key, out T value)
        {
			var found = base.TryGetValue(key, out value);
			if (found)
            {
				CacheUsedCounter++;
            }
            else
            {
				CacheNotUsedCounter++;
            }
			return found;
        }

        public override void Remove(string key)
        {
			RemoveEntryCounter++;
			base.Remove(key);
        }
	}
}
