using QoDL.Toolkit.Core.Util;
using System;
using System.Collections.Generic;
using System.IO;

namespace QoDL.Toolkit.Core.Models;

/// <summary>
/// Information about the current request.
/// </summary>
public class RequestInformation<TAccessRole>
{
    /// <summary>
    /// Access roles of the current request.
    /// </summary>
    public Maybe<TAccessRole> AccessRole { get; set; }

    /// <summary>
    /// Optional for auditing if enabled.
    /// </summary>
    public string UserId { get; set; }

    /// <summary>
    /// Optional for auditing if enabled.
    /// </summary>
    public string UserName { get; set; }

    /// <summary>
    /// Is set to the current tokens id when using an access token.
    /// </summary>
    public Guid? CurrentTokenId { get; set; }

    /// <summary>
    /// True if a token is being used that allows killswitching.
    /// </summary>
    public bool AllowAccessTokenKillswitch { get; set; }

    /// <summary>
    /// True when using an access token.
    /// </summary>
    public bool IsUsingAccessToken => CurrentTokenId != null;

    /// <summary>
    /// Is set automatically to the full url of the request.
    /// </summary>
    public string Url { get; set; }

    /// <summary>
    /// Is set automatically to all header keys and values.
    /// </summary>
    public Dictionary<string, string> Headers { get; set; }

    /// <summary>
    /// POST, GET etc
    /// </summary>
    public string Method { get; set; }

    /// <summary>
    /// Client ip address.
    /// </summary>
    public string ClientIP { get; set; }

    internal Stream InputStream { get; set; }
    internal List<RequestFormFile> FormFiles { get; set; } = new List<RequestFormFile>();

    /// <summary>
    /// Create a new <see cref="RequestInformation{TAccessRole}"/> object.
    /// </summary>
    /// <param name="accessRole">Roles the current request has.</param>
    /// <param name="userId">Optional user id reference if any. Optionally used for auditing.</param>
    /// <param name="userName">Optional user name reference if any. Optionally used for auditing.</param>
    public RequestInformation(Maybe<TAccessRole> accessRole, string userId = null, string userName = null)
    {
        AccessRole = accessRole;
        UserId = userId;
        UserName = userName;
    }

    /// <summary>
    /// Create a new <see cref="RequestInformation{TAccessRole}"/> object.
    /// </summary>
    /// <param name="accessRole">Roles the current request has.</param>
    /// <param name="userId">Optional user id reference if any. Optionally used for auditing.</param>
    /// <param name="userName">Optional user name reference if any. Optionally used for auditing.</param>
    public RequestInformation(TAccessRole accessRole, string userId = null, string userName = null)
        : this(new Maybe<TAccessRole>(accessRole), userId, userName) { }
}
