using System.IdentityModel.Tokens;

namespace Stardust.Interstellar
{
    /// <summary>
    /// Provides an extension point to add a custom token cache
    /// </summary>
    public interface ITokenManagerCache
    {

        /// <summary>
        /// Add a token to the cache
        /// </summary>
        /// <param name="tokenKey">The cache key for the token</param>
        /// <param name="token">The token to cache</param>
        void AddTokenToCache(string tokenKey, SecurityToken token);

        /// <summary>
        /// Tries to get a token from the cache.
        /// </summary>
        /// <param name="tokenKey">The cache key for the token</param>
        /// <param name="cachedToken">The cached token. <see langword="null"/> if not found</param>
        /// <returns>true if token is cached and valid.</returns>
        bool TryGetToken(string tokenKey, out SecurityToken cachedToken);

        /// <summary>
        /// Removes all expired tokens from the cache
        /// </summary>
        void InvalidateTokenCache();
    }

    //class TokenManagerCache : ITokenManagerCache
    //{
    //}
}