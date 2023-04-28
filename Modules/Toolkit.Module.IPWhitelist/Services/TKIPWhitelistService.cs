using QoDL.Toolkit.Module.IPWhitelist.Abstractions;
using QoDL.Toolkit.Module.IPWhitelist.Models;
using QoDL.Toolkit.Module.IPWhitelist.Utils;
using QoDL.Toolkit.Core.Util;
using QoDL.Toolkit.Core.Models;
using System.Threading.Tasks;
using System.Linq;
using System;

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

    /// <summary></summary>
    public virtual bool IsEnabled() => Enabled;

#if NETCORE
    /// <summary></summary>
    public Task<TKIPWhitelistCheckResult> HandleRequestAsync(HttpContext context)
    {
        var rawIp = RequestUtils.GetIPAddress(context);
        var requestIp = TKIPAddressUtils.ParseIP(rawIp);
        var path = RequestUtils.GetPath(context.Request);
        return HandleRequestAsync(requestIp, path);
    }
#endif

#if NETFULL
    /// <summary></summary>
    public Task<TKIPWhitelistCheckResult> HandleRequestAsync(HttpContext context)
    {
        var rawIp = RequestUtils.GetIPAddress(new HttpRequestWrapper(context.Request));
        var requestIp = TKIPAddressUtils.ParseIP(rawIp);
        var path = RequestUtils.GetPath(context.Request);
        return HandleRequestAsync(requestIp, path);
    }
#endif

    /// <summary></summary>
    public virtual async Task<TKIPWhitelistCheckResult> HandleRequestAsync(TKIPData ip, string path, bool testMode = false)
    {
        if (!testMode && (!Enabled || ip.IsLocalHost && DisableForLocalhost)) return TKIPWhitelistCheckResult.CreateAllowed("IP whitelist disabled or localhost.");
        else if (ShouldIgnorePath != null && await ShouldIgnorePath(path)) return TKIPWhitelistCheckResult.CreateAllowed($"Path '{path}' is ignored by config.");

        var rules = await _whitelistRuleStorage.GetRulesAsync();
        var allowingRule = rules?.FirstOrDefault(r => RuleContainsWhitelistFor(r, ip.IP));
        if (allowingRule != null) return TKIPWhitelistCheckResult.CreateAllowed("Matching whitelist rule.", allowingRule);

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
}
