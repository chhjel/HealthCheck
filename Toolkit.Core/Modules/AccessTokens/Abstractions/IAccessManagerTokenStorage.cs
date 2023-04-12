using QoDL.Toolkit.Core.Modules.AccessTokens.Models;
using System;
using System.Collections.Generic;

namespace QoDL.Toolkit.Core.Modules.AccessTokens.Abstractions;

/// <summary>
/// Provides storage for generated access tokens.
/// </summary>
public interface IAccessManagerTokenStorage
{
    /// <summary>
    /// Get all tokens.
    /// </summary>
    IEnumerable<TKAccessToken> GetTokens();

    /// <summary>
    /// Get a single token by id.
    /// </summary>
    TKAccessToken GetToken(Guid id);

    /// <summary>
    /// Save the given token.
    /// </summary>
    TKAccessToken SaveNewToken(TKAccessToken token);

    /// <summary>
    /// Update token last used at datetime.
    /// <para>Called at max once per minute per token.</para>
    /// </summary>
    TKAccessToken UpdateTokenLastUsedAtTime(Guid id, DateTimeOffset time);

    /// <summary>
    /// Deletes the token with the given id.
    /// </summary>
    void DeleteToken(Guid tokenId);
}
