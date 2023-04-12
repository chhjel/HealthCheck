using System;
using System.Collections.Generic;

namespace QoDL.Toolkit.Core.Modules.AccessTokens.Models;

/// <summary>
/// A generated access token.
/// </summary>
public class TKAccessToken
{
    /// <summary>
    /// Unique id for this token.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Name that was given to the token.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// When true, the token can be destroyed from the profile dialog if enabled when used.
    /// </summary>
    public bool AllowKillswitch { get; set; }

    /// <summary>
    /// Hash of the token value.
    /// </summary>
    public string HashedToken { get; set; }

    /// <summary>
    /// Salt for the hashed token.
    /// </summary>
    public string TokenSalt { get; set; }

    /// <summary>
    /// Date of creation.
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>
    /// Expiration date if any.
    /// </summary>
    public DateTimeOffset? ExpiresAt { get; set; }

    /// <summary>
    /// Last used date.
    /// <para>Updated if more than 1 min since last time.</para>
    /// </summary>
    public DateTimeOffset? LastUsedAt { get; set; }

    /// <summary>
    /// Roles the token will be granted.
    /// </summary>
    public List<string> Roles { get; set; }

    /// <summary>
    /// Modules the token has access to.
    /// </summary>
    public List<TKAccessTokenModuleData> Modules { get; set; }
}
