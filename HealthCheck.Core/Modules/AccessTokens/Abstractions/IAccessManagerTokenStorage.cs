using HealthCheck.Core.Modules.AccessTokens.Models;
using System;
using System.Collections.Generic;

namespace HealthCheck.Core.Modules.AccessTokens.Abstractions
{
    /// <summary>
    /// Provides storage for generated access tokens.
    /// </summary>
    public interface IAccessManagerTokenStorage
    {
        /// <summary>
        /// Get all tokens.
        /// </summary>
        IEnumerable<HCAccessToken> GetTokens();

        /// <summary>
        /// Get a single token by id.
        /// </summary>
        HCAccessToken GetToken(Guid id);

        /// <summary>
        /// Save the given token.
        /// </summary>
        HCAccessToken SaveNewToken(HCAccessToken token);

        /// <summary>
        /// Update token last used at datetime.
        /// </summary>
        HCAccessToken UpdateTokenLastUsedAtTime(Guid id, DateTime time);

        /// <summary>
        /// Deletes the token with the given id.
        /// </summary>
        void DeleteToken(Guid tokenId);
    }
}
