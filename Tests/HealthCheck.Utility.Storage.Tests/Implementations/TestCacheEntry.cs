using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;


namespace HealthCheck.Utility.Storage.Tests.Implementations
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Major Code Smell", "S3881:\"IDisposable\" should be implemented correctly", Justification = "Test")]
	public class TestCacheEntry : ICacheEntry
	{
		public object Key { get; set; }

		public object Value { get; set; }
		public DateTimeOffset? AbsoluteExpiration { get; set; }
		public TimeSpan? AbsoluteExpirationRelativeToNow { get; set; }
		public TimeSpan? SlidingExpiration { get; set; }

		public IList<IChangeToken> ExpirationTokens { get; set; }

		public IList<PostEvictionCallbackRegistration> PostEvictionCallbacks { get; set; }

		public CacheItemPriority Priority { get; set; }
		public long? Size { get; set; }
		public void Dispose() { /*Ignored*/ }
	}
}
