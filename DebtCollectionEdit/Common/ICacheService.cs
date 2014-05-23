using System;

namespace DebtCollection.Common
{
    /// <summary>
    /// Interface for cache service.
    /// </summary>
    public interface ICacheService
    {
        /// <summary>
        /// Gets values from cache.
        /// </summary>
        /// <permission cref="System.Security.PermissionSet">public</permission>
        /// <param name="cacheKey">Cache key.</param>
        /// <typeparam name="T">Type of value put in cache.</typeparam>
        /// <returns>Returns value from cache.</returns>
        T Get<T>(String cacheKey) where T : class;

        /// <summary>
        /// Purge value from cache.
        /// </summary>
        /// <permission cref="System.Security.PermissionSet">public</permission>
        /// <param name="cacheKey">Value cache key to purge from cache.</param>
        void Purge(String cacheKey);

        /// <summary>
        /// Stores value in cache.
        /// </summary>
        /// <permission cref="System.Security.PermissionSet">public</permission>
        /// <param name="cacheKey">Cache key.</param>
        /// <param name="value">Object to store in cache.</param>
        /// <typeparam name="T">Type of value put in cache.</typeparam>
        void Set<T>(String cacheKey, T value) where T : class;
    }
}