using HealthCheck.Core.Modules.AccessManager.Models;
using System;
using System.Collections.Generic;

namespace HealthCheck.Core.Modules.AccessManager.Abstractions
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
        /// Deletes the token with the given id.
        /// </summary>
        void DeleteConfig(Guid tokenId);
    }
}
