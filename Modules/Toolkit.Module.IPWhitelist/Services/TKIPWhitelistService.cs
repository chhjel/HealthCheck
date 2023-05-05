using QoDL.Toolkit.Core.Extensions;
using QoDL.Toolkit.Core.Util;
using QoDL.Toolkit.Core.Util.Collections;
using QoDL.Toolkit.Module.IPWhitelist.Abstractions;
using QoDL.Toolkit.Module.IPWhitelist.Models;
using QoDL.Toolkit.Module.IPWhitelist.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

#if NETCORE
using Microsoft.AspNetCore.Http;
#endif

namespace QoDL.Toolkit.Module.IPWhitelist.Services;

/// <summary></summary>
public class TKIPWhitelistServiceOptions
{
    /// <summary>Defaults to true</summary>
    public bool Enabled { get; set; } = true;

    /// <summary>Defaults to true</summary>
    public bool DisableForLocalhost { get; set; } = true;

    /// <summary></summary>
    public string BlockedPageTitle { get; set; }

    /// <summary>
    /// Defaults to true, whitelists urls to add new whitelist ips.
    /// </summary>
    public bool AlwaysAllowWhitelistLinkUrls { get; set; } = true;

    /// <summary>
    /// Optional check if a given url path should be ignored and not blocked.
    /// <para>Should contain logic to allow e.g. login and toolkit endpoints if needed.</para>
    /// </summary>
    public PathConditionDelegate ShouldAlwaysAllowRequest { get; set; }
    /// <summary></summary>
    public delegate Task<bool> PathConditionDelegate(TKIPWhitelistRequestData request);
}

/// <summary></summary>
public class TKIPWhitelistService : ITKIPWhitelistService
{
    private readonly TKIPWhitelistServiceOptions _options;
    private readonly ITKIPWhitelistRuleStorage _whitelistRuleStorage;
    private readonly ITKIPWhitelistConfigStorage _whitelistConfigStorage;
    private readonly ITKIPWhitelistIPStorage _whitelistIPStorage;

    /// <summary></summary>
    public TKIPWhitelistService(TKIPWhitelistServiceOptions options, ITKIPWhitelistRuleStorage whitelistRuleStorage, ITKIPWhitelistConfigStorage whitelistConfigStorage, ITKIPWhitelistIPStorage whitelistIPStorage)
    {
        _options = options;
        _whitelistRuleStorage = whitelistRuleStorage;
        _whitelistConfigStorage = whitelistConfigStorage;
        _whitelistIPStorage = whitelistIPStorage;
    }

    /// <inheritdoc/>
    public virtual bool IsEnabled() => _options.Enabled;

#if NETCORE
    /// <inheritdoc/>
    public Task<TKIPWhitelistCheckResult> HandleRequestAsync(HttpContext context)
    {
        var data = new TKIPWhitelistRequestData
        {
            IP = RequestUtils.GetIPAddress(context),
            Path = RequestUtils.GetPath(context.Request),
            PathAndQuery = RequestUtils.GetPathAndQuery(context.Request),
            Context = context
        };
        return HandleRequestAsync(data);
    }
#endif

#if NETFULL
    /// <inheritdoc/>
    public Task<TKIPWhitelistCheckResult> HandleRequestAsync(HttpRequest request)
    {
        var data = new TKIPWhitelistRequestData
        {
            IP = RequestUtils.GetIPAddress(new HttpRequestWrapper(request)),
            Path = RequestUtils.GetPath(request),
            PathAndQuery = RequestUtils.GetPathAndQuery(request),
            Request = request
        };
        return HandleRequestAsync(data);
    }

    /// <inheritdoc/>
    public Task<TKIPWhitelistCheckResult> HandleRequestAsync(HttpRequestMessage request)
    {
        var data = new TKIPWhitelistRequestData
        {
            IP = RequestUtils.GetIPAddress(request),
            Path = RequestUtils.GetPath(request),
            PathAndQuery = RequestUtils.GetPathAndQuery(request),
            WebApiRequest = request
        };
        return HandleRequestAsync(data);
    }
#endif

    /// <summary></summary>
    protected virtual async Task<TKIPWhitelistCheckResult> HandleRequestAsync(TKIPWhitelistRequestData request)
    {
        var result = await IsRequestAllowedAsync(request);
        AddLog(new TKIPWhitelistLogItem
        {
            IP = request.IP,
            Path = request.PathAndQuery,
            Timestamp = DateTimeOffset.Now,
            WasBlocked = result.Blocked,
            Note = result.Blocked ? "Blocked." : result.AllowedReason
        });

        result.Response = string.IsNullOrWhiteSpace(result.Response) ? string.Empty : CreateBlockedPageHtml(result);
        return result;
    }

    /// <summary>Optionally override blocked page html.</summary>
    protected virtual string CreateBlockedPageHtml(TKIPWhitelistCheckResult result)
    {
        if (!result.UseDefaultResponseWrapper) return result.Response;

        return $@"
<!doctype html>
<html>
<head>
    <title>{HttpUtility.HtmlEncode(_options.BlockedPageTitle ?? "403")}</title>
    <meta charset=""utf-8"">
    <meta name=""robots"" content=""noindex"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1, maximum-scale=1, user-scalable=no, minimal-ui"">
    <meta name=""robots"" content=""noindex"">
    <style>
        a {{ color: #ffc182; }}
    </style>
</head>

<body style=""margin: 0; background-color: #121212; color: #eee; font-family: sans-serif; font-size: 22px;"">
    <div style=""max-width: 800px; margin: auto; padding: 40px;"">
        <div style=""padding: 40px; background-color: #2c2a27; border: 3px solid #3c3c3c;"">
            {result.Response}
        </div>
    </div>
</body>
</html>";
    }

    /// <summary></summary>
    public virtual async Task<TKIPWhitelistCheckResult> IsRequestAllowedAsync(TKIPWhitelistRequestData request, bool testMode = false)
    {
        var ip = TKIPAddressUtils.ParseIP(request.IP, acceptLocalhostString: true);

        if (!testMode && !_options.Enabled) return TKIPWhitelistCheckResult.CreateAllowed("IP whitelist disabled.");
        else if (!testMode && ip.IsLocalHost && _options.DisableForLocalhost) return TKIPWhitelistCheckResult.CreateAllowed("IP whitelist disabled for localhost request.");
        else if (_options.AlwaysAllowWhitelistLinkUrls && RequestIsWhitelistLink(request)) return TKIPWhitelistCheckResult.CreateAllowed("Request seems to be a whitelist link url and was allowed by AlwaysAllowWhitelistLinkUrls-config.");
        else if (_options.ShouldAlwaysAllowRequest != null && await _options.ShouldAlwaysAllowRequest(request)) return TKIPWhitelistCheckResult.CreateAllowed($"Request was allowed by ShouldAlwaysAllowRequest-config.");

        var rules = await _whitelistRuleStorage.GetRulesAsync();
        await EnsureIPsCachedAsync();
        var allowingRule = rules?.FirstOrDefault(r => RuleContainsWhitelistFor(r, ip.IP));
        if (allowingRule != null) return TKIPWhitelistCheckResult.CreateAllowed($"Matching whitelist rule '{allowingRule.Name}'", allowingRule);

        var config = (await _whitelistConfigStorage.GetConfigAsync()) ?? new();

        return TKIPWhitelistCheckResult.CreateBlocked(
            config?.DefaultResponse ?? "⛔ 403 ⛔",
            config?.DefaultHttpStatusCode ?? (int)System.Net.HttpStatusCode.Forbidden,
            config?.UseDefaultResponseWrapper != null ? config.UseDefaultResponseWrapper : true
        );
    }

    private static List<string> _whitelistLinkUrls = new List<string>
    {
        "/IPWLLink/",
        "/IPWLLinkActivate/",
        "/GetMainStyle",
        "/GetMainScript"
    };
    private bool RequestIsWhitelistLink(TKIPWhitelistRequestData request)
        => _whitelistLinkUrls.Any(x => request?.Path?.ToLower()?.Contains(x.ToLower()) == true);

    /// <summary></summary>
    protected virtual bool RuleContainsWhitelistFor(TKIPWhitelistRule rule, string ip)
    {
        // Rule disabled => nope
        if (rule?.Enabled != true) return false;
        // Rule expired => nope
        else if (rule?.EnabledUntil != null && rule.EnabledUntil < DateTimeOffset.Now) return false;
        // Check for IP match
        else return GetRuleIPsCached(rule).TryAny(ruleIP => TKIPAddressUtils.IpMatchesOrIsWithinCidrRange(ip, ruleIP.IP)) == true;
    }

    #region IP cache
    private static readonly SemaphoreSlim _ipCacheLock = new(1, 1);
    private static DateTimeOffset _ipsCachedAt = DateTimeOffset.UtcNow.AddDays(-1);
    private static readonly ConcurrentDictionary<Guid, List<TKIPWhitelistIP>> _ipCache = new();
    
    /// <inheritdoc />
    public async Task InvalidateIPCacheAsync()
    {
        await _ipCacheLock.WaitAsync();
        try
        {
            _ipCache.Clear();
            _ipsCachedAt = DateTimeOffset.UtcNow.AddDays(-1);
        }
        finally
        {
            _ipCacheLock.Release();
        }
    }

    private async Task EnsureIPsCachedAsync()
    {
        await _ipCacheLock.WaitAsync();
        try
        {
            if ((DateTimeOffset.UtcNow - _ipsCachedAt) > TimeSpan.FromSeconds(30))
            {
                var ips = await _whitelistIPStorage.GetAllIPsAsync();
                _ipCache.Clear();
                foreach (var ip in ips)
                {
                    if (!_ipCache.ContainsKey(ip.RuleId)) _ipCache[ip.RuleId] = new();
                    _ipCache[ip.RuleId].Add(ip);
                }
            }
        }
        finally
        {
            _ipCacheLock.Release();
        }
    }

    private IEnumerable<TKIPWhitelistIP> GetRuleIPsCached(TKIPWhitelistRule rule)
        => _ipCache.TryGetValue(rule.Id, out var list) ? list : Enumerable.Empty<TKIPWhitelistIP>();
    #endregion

    #region Request logs
    private static readonly object _logLock = new();
    private static readonly TKLimitedList<TKIPWhitelistLogItem> _allowedLog = new(500);
    private static readonly TKLimitedList<TKIPWhitelistLogItem> _blockedLog = new(500);

    /// <summary>
    /// Override the default request log limit (latest 500 allowed, 500 blocked).
    /// </summary>
    public static void SetLogSize(int maxCount)
    {
        lock (_logLock)
        {
            _allowedLog.MaxItemLimit = maxCount;
            _blockedLog.MaxItemLimit = maxCount;
        }
    }

    /// <inheritdoc />
    public void ClearLogs()
    {
        lock (_logLock)
        {
            _allowedLog.Clear();
            _blockedLog.Clear();
        }
    }

    private static void AddLog(TKIPWhitelistLogItem entry)
    {
        lock (_logLock)
        {
            if (entry.WasBlocked) _blockedLog.Add(entry);
            else _allowedLog.Add(entry);
        }
    }

    /// <inheritdoc/>
    public IEnumerable<TKIPWhitelistLogItem> GetLog()
    {
        lock (_logLock)
        {
            return _allowedLog
                .Concat(_blockedLog)
                .OrderByDescending(x => x.Timestamp)
                .ToList();
        }
    }
    #endregion
}
