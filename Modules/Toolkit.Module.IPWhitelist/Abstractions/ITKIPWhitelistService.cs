using QoDL.Toolkit.Module.IPWhitelist.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

#if NETFULL
using System.Web;
#endif

#if NETCORE
using Microsoft.AspNetCore.Http;
#endif

namespace QoDL.Toolkit.Module.IPWhitelist.Abstractions;

/// <summary>
/// Handles checking if a request should be blocked or not.
/// </summary>
public interface ITKIPWhitelistService
{
    /// <summary>
    /// If false, nothing happens.
    /// </summary>
    bool IsEnabled();

#if NETFULL
    /// <summary>
    /// Handle the given context and check if it should be blocked.
    /// </summary>
    Task<TKIPWhitelistCheckResult> HandleRequestAsync(HttpContext context);
#endif

#if NETCORE
    /// <summary>
    /// Handle the given context and check if it should be blocked.
    /// </summary>
    Task<TKIPWhitelistCheckResult> HandleRequestAsync(HttpContext context);
#endif

    /// <summary>
    /// Check if request is allowed.
    /// <para>Called from <see cref="HandleRequestAsync"/>, used for test mode.</para>
    /// </summary>
    Task<TKIPWhitelistCheckResult> IsRequestAllowedAsync(string rawIp, string path, bool testMode = false);

    /// <summary>
    /// Get a log of recently blocked/allowed items.
    /// </summary>
    IEnumerable<TKIPWhitelistLogItem> GetLog();
}
