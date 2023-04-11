using System;

namespace QoDL.Toolkit.Core.Util
{
    /// <summary>
    /// Invalidates cache on other instances when needed.
    /// </summary>
    public abstract class TKSimpleMemoryCacheLoadBalancedBase : TKSimpleMemoryCache
    {
        /// <inheritdoc />
        public override T SetValue<T>(string key, T value, DateTime expiresAt, bool allowDistribute = false)
        {
            if (allowDistribute)
            {
                InvalidateCacheOnOtherInstances(key);
            }
            return base.SetValue(key, value, expiresAt, allowDistribute);
        }

        /// <inheritdoc />
        public override void ClearKey(string key, bool allowDistribute = false)
        {
            if (allowDistribute)
            {
                InvalidateCacheOnOtherInstances(key);
            }
            base.ClearKey(key, allowDistribute);
        }

        /// <inheritdoc />
        public override void ClearAll(bool allowDistribute = false)
        {
            if (allowDistribute)
            {
                InvalidateCacheOnOtherInstances();
            }
            base.ClearAll(allowDistribute);
        }

        /// <summary>
        /// Handles invalidating the cache for the given key or all keys if null on all other instances.
        /// </summary>
        public abstract void InvalidateCacheOnOtherInstances(string key = null);
    }
}
