using QoDL.Toolkit.Module.IPWhitelist.Abstractions;
using QoDL.Toolkit.Module.IPWhitelist.Models;
using QoDL.Toolkit.Module.IPWhitelist.Utils;
using QoDL.Toolkit.Core.Util;
using System.Threading.Tasks;
using System.Linq;
using System;
using QoDL.Toolkit.Core.Util.Collections;
using System.Collections.Generic;
using System.Web;

#if NETFULL
using System.Web;
#endif

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

    /// <summary>Optional check if a given url path should be ignored and not blocked.</summary>
    public TKIPWhitelistService.PathConditionDelegate ShouldIgnorePath { get; set; }
}

/// <summary></summary>
public class TKIPWhitelistService : ITKIPWhitelistService
{
    /// <summary></summary>
    public delegate Task<bool> PathConditionDelegate(string path);

    private readonly TKIPWhitelistServiceOptions _options;
    private readonly ITKIPWhitelistRuleStorage _whitelistRuleStorage;
    private readonly ITKIPWhitelistConfigStorage _whitelistConfigStorage;

    /// <summary></summary>
    public TKIPWhitelistService(TKIPWhitelistServiceOptions options, ITKIPWhitelistRuleStorage whitelistRuleStorage, ITKIPWhitelistConfigStorage whitelistConfigStorage)
    {
        _options = options;
        _whitelistRuleStorage = whitelistRuleStorage;
        _whitelistConfigStorage = whitelistConfigStorage;
    }

    /// <inheritdoc/>
    public virtual bool IsEnabled() => _options.Enabled;

#if NETCORE
    /// <inheritdoc/>
    public Task<TKIPWhitelistCheckResult> HandleRequestAsync(HttpContext context)
    {
        var rawIp = RequestUtils.GetIPAddress(context);
        var path = RequestUtils.GetPathAndQuery(context.Request);
        return HandleRequestAsync(rawIp, path);
    }
#endif

#if NETFULL
    /// <inheritdoc/>
    public Task<TKIPWhitelistCheckResult> HandleRequestAsync(HttpContext context)
    {
        var rawIp = RequestUtils.GetIPAddress(new HttpRequestWrapper(context.Request));
        var path = RequestUtils.GetPathAndQuery(context.Request);
        return HandleRequestAsync(rawIp, path);
    }
#endif

    /// <summary></summary>
    protected virtual async Task<TKIPWhitelistCheckResult> HandleRequestAsync(string rawIp, string path)
    {
        var result = await IsRequestAllowedAsync(rawIp, path);
        AddLog(new TKIPWhitelistLogItem
        {
            IP = rawIp,
            Path = path,
            Timestamp = DateTimeOffset.Now,
            WasBlocked = result.Blocked,
            Note = result.Blocked ? "Blocked." : result.AllowedReason
        });

        result.Response = CreateBlockedPageHtml(result);
        return result;
    }

    /// <summary>Optionally override blocked page html.</summary>
    protected virtual string CreateBlockedPageHtml(TKIPWhitelistCheckResult result)
    => $@"
<!doctype html>
<html>
<head>
    <title>{HttpUtility.HtmlEncode(_options.BlockedPageTitle ?? "403")}</title>
    <meta charset=""utf-8"">
    <meta name=""robots"" content=""noindex"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1, maximum-scale=1, user-scalable=no, minimal-ui"">
    <meta name=""robots"" content=""noindex"">
</head>

<body style=""margin: 0; background-color: #121212; color: #eee; font-family: sans-serif; font-size: 22px;"">
    <div style=""max-width: 800px; margin: auto; padding: 40px;"">
        <div style=""padding: 40px; background-color: #2c2a27; border: 3px solid #3c3c3c;"">
            {result.Response}
        </div>
    </div>
</body>
</html>";

    /// <summary></summary>
    public virtual async Task<TKIPWhitelistCheckResult> IsRequestAllowedAsync(string rawIp, string path, bool testMode = false)
    {
        var ip = TKIPAddressUtils.ParseIP(rawIp, acceptLocalhostString: true);

        if (!testMode && !_options.Enabled) return TKIPWhitelistCheckResult.CreateAllowed("IP whitelist disabled.");
        else if (!testMode && ip.IsLocalHost && _options.DisableForLocalhost) return TKIPWhitelistCheckResult.CreateAllowed("IP whitelist disabled for localhost request.");
        else if (_options.ShouldIgnorePath != null && await _options.ShouldIgnorePath(path)) return TKIPWhitelistCheckResult.CreateAllowed($"Path '{path}' is ignored by config.");

        var rules = await _whitelistRuleStorage.GetRulesAsync();
        var allowingRule = rules?.FirstOrDefault(r => RuleContainsWhitelistFor(r, ip.IP));
        if (allowingRule != null) return TKIPWhitelistCheckResult.CreateAllowed($"Matching whitelist rule '{allowingRule.Name}'", allowingRule);

        var config = (await _whitelistConfigStorage.GetConfigAsync()) ?? new();

        return TKIPWhitelistCheckResult.CreateBlocked(
            config?.DefaultResponse ?? "⛔ 403 ⛔",
            config?.DefaultHttpStatusCode ?? (int)System.Net.HttpStatusCode.Forbidden
        );
    }

    /// <summary></summary>
    protected virtual bool RuleContainsWhitelistFor(TKIPWhitelistRule rule, string ip)
    {
        // Rule disabled => nope
        if (rule?.Enabled != true) return false;
        // Rule expired => nope
        else if (rule?.EnabledUntil != null && rule.EnabledUntil < DateTimeOffset.Now) return false;
        // Check for IP match
        else return rule?.Ips?.Any(ruleIP => TKIPAddressUtils.IpMatchesOrIsWithinCidrRange(ip, ruleIP)) == true;
    }

    private static readonly TKLimitedList<TKIPWhitelistLogItem> _log = new(100);
    private static void AddLog(TKIPWhitelistLogItem entry)
    {
        lock (_log)
        {
            _log.Add(entry);
        }
    }

    /// <inheritdoc/>
    public IEnumerable<TKIPWhitelistLogItem> GetLog()
    {
        lock (_log)
        {
            return _log.ToList();
        }
    }
}
