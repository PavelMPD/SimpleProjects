using System;
using Microsoft.Practices.EnterpriseLibrary.Caching;
using Microsoft.Practices.EnterpriseLibrary.Caching.Expirations;
using Microsoft.Practices.EnterpriseLibrary.Common.Configuration;

namespace DebtCollection.Common
{
    /// <summary>
    /// Cache service implementation on base of Enterprise Library cache block.
    /// </summary>
    public class ElCacheService : ICacheService
    {
        /// <summary>
        /// Stores initialized cache manager.
        /// </summary>
        /// <permission cref="System.Security.PermissionSet">public</permission>
        private static ICacheManager defaultCache = EnterpriseLibraryContainer.Current.GetInstance<ICacheManager>();

        private const int CacheTime = 600;

        /// <summary>
        /// Gets default cache manager instance.
        /// </summary>
        /// <permission cref="System.Security.PermissionSet">public</permission>
        private ICacheManager DefaultCache
        {
            get
            {
                return defaultCache;
            }
        }

        /// <summary>
        /// Gets values from cache.
        /// </summary>
        /// <permission cref="System.Security.PermissionSet">public</permission>
        /// <param name="cacheKey">Cache key.</param>
        /// <typeparam name="T">Type of value put in cache.</typeparam>
        /// <returns>Returns value from cache.</returns>
        public T Get<T>(String cacheKey) where T : class
        {
            return DefaultCache.GetData(cacheKey) as T;
        }

        /// <summary>
        /// Purge value from cache.
        /// </summary>
        /// <permission cref="System.Security.PermissionSet">public</permission>
        /// <param name="cacheKey">Value cache key to purge from cache.</param>
        public void Purge(String cacheKey)
        {
            if (DefaultCache.Contains(cacheKey))
            {
                DefaultCache.Remove(cacheKey);
            }
        }

        /// <summary>
        /// Stores value in cache.
        /// </summary>
        /// <permission cref="System.Security.PermissionSet">public</permission>
        /// <param name="cacheKey">Cache key.</param>
        /// <param name="value">Object to store in cache.</param>
        /// <typeparam name="T">Type of value put in cache.</typeparam>
        public void Set<T>(String cacheKey, T value) where T : class
        {
            DefaultCache.Add(cacheKey, value, CacheItemPriority.Normal, null, new AbsoluteTime(DateTime.Now.AddSeconds(CacheTime)));
        }
    }
}