using QoDL.Toolkit.Module.IPWhitelist.Abstractions;
using QoDL.Toolkit.Module.IPWhitelist.Models;
using QoDL.Toolkit.Module.IPWhitelist.Utils;
using QoDL.Toolkit.Core.Util;
using System.Threading.Tasks;
using System.Linq;
using System;
using QoDL.Toolkit.Core.Util.Collections;
using System.Collections.Generic;

#if NETFULL
using System.Web;
#endif

#if NETCORE
using Microsoft.AspNetCore.Http;
#endif

namespace QoDL.Toolkit.Module.IPWhitelist.Services;

/// <summary></summary>
public class TKIPWhitelistService : ITKIPWhitelistService
{
    /// <summary>Defaults to true</summary>
    public bool Enabled { get; set; } = true;

    /// <summary>Defaults to true</summary>
    public bool DisableForLocalhost { get; set; } = true;

    /// <summary></summary>
    public delegate Task<bool> PathConditionDelegate(string path);
    /// <summary>Optional check if a given url path should be ignored and not blocked.</summary>
    public PathConditionDelegate ShouldIgnorePath { get; set; }

    private readonly ITKIPWhitelistRuleStorage _whitelistRuleStorage;
    private readonly ITKIPWhitelistConfigStorage _whitelistConfigStorage;

    /// <summary></summary>
    public TKIPWhitelistService(ITKIPWhitelistRuleStorage whitelistRuleStorage, ITKIPWhitelistConfigStorage whitelistConfigStorage)
    {
        _whitelistRuleStorage = whitelistRuleStorage;
        _whitelistConfigStorage = whitelistConfigStorage;
    }

    /// <inheritdoc/>
    public virtual bool IsEnabled() => Enabled;

#if NETCORE
    /// <inheritdoc/>
    public Task<TKIPWhitelistCheckResult> HandleRequestAsync(HttpContext context)
    {
        var rawIp = RequestUtils.GetIPAddress(context);
        var path = RequestUtils.GetPath(context.Request);
        return HandleRequestAsync(rawIp, path);
    }
#endif

#if NETFULL
    /// <inheritdoc/>
    public Task<TKIPWhitelistCheckResult> HandleRequestAsync(HttpContext context)
    {
        var rawIp = RequestUtils.GetIPAddress(new HttpRequestWrapper(context.Request));
        var path = RequestUtils.GetPath(context.Request);
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
        return result;
    }

    /// <summary></summary>
    public virtual async Task<TKIPWhitelistCheckResult> IsRequestAllowedAsync(string rawIp, string path, bool testMode = false)
    {
        var ip = TKIPAddressUtils.ParseIP(rawIp);

        if (!testMode && (!Enabled || ip.IsLocalHost && DisableForLocalhost)) return TKIPWhitelistCheckResult.CreateAllowed("IP whitelist disabled or localhost.");
        else if (ShouldIgnorePath != null && await ShouldIgnorePath(path)) return TKIPWhitelistCheckResult.CreateAllowed($"Path '{path}' is ignored by config.");

        var rules = await _whitelistRuleStorage.GetRulesAsync();
        var allowingRule = rules?.FirstOrDefault(r => RuleContainsWhitelistFor(r, ip.IP));
        if (allowingRule != null) return TKIPWhitelistCheckResult.CreateAllowed($"Matching whitelist rule '{allowingRule.Name}'", allowingRule);

        var config = await _whitelistConfigStorage.GetConfigAsync();

        return TKIPWhitelistCheckResult.CreateBlocked(
            config?.DefaultResponse ?? "⛔",
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
    private void AddLog(TKIPWhitelistLogItem entry)
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
