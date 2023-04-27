using QoDL.Toolkit.Module.IPWhitelist.Abstractions;
using QoDL.Toolkit.Module.IPWhitelist.Models;
using QoDL.Toolkit.Module.IPWhitelist.Utils;
using QoDL.Toolkit.Core.Util;
using QoDL.Toolkit.Core.Models;
using System.Threading.Tasks;
using System.Linq;

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

    private readonly ITKIPWhitelistRuleStorage _whitelistRuleStorage;

    /// <summary></summary>
    public TKIPWhitelistService(ITKIPWhitelistRuleStorage whitelistRuleStorage)
    {
        _whitelistRuleStorage = whitelistRuleStorage;
    }

    /// <summary></summary>
    public virtual bool IsEnabled() => Enabled;

#if NETCORE
    /// <summary></summary>
    public virtual Task<TKIPWhitelistCheckResult> HandleRequestAsync(HttpContext context)
    {
        var rawIp = RequestUtils.GetIPAddress(context);
        var requestIp = TKIPAddressUtils.ParseIP(rawIp);
        var path = RequestUtils.GetUrl(context.Request);
        return HandleRequestAsync(requestIp, path);
    }
#endif

#if NETFULL
    /// <summary></summary>
    public virtual Task<TKIPWhitelistCheckResult> HandleRequestAsync(HttpContext context)
    {
        var rawIp = RequestUtils.GetIPAddress(new HttpRequestWrapper(context.Request));
        var requestIp = TKIPAddressUtils.ParseIP(rawIp);
        var path = RequestUtils.GetUrl(context.Request);
        return HandleRequestAsync(requestIp, path);
    }
#endif

    /// <summary></summary>
    protected virtual async Task<TKIPWhitelistCheckResult> HandleRequestAsync(TKIPData ip, string path)
    {
        if (!Enabled || ip.IsLocalHost && DisableForLocalhost) return new TKIPWhitelistCheckResult { Blocked = false };

        var rules = await _whitelistRuleStorage.GetRulesAsync();
        var shouldBlock = rules?.Any(r => RuleContainsWhitelistFor(r, ip.IP)) == true;

        if (shouldBlock) return new TKIPWhitelistCheckResult { Blocked = true };
        else return new TKIPWhitelistCheckResult { Blocked = false };
    }

    /// <summary></summary>
    protected virtual bool RuleContainsWhitelistFor(TKIPWhitelistRule rule, string ip)
        => rule?.Ips?.Any(ruleIP => TKIPAddressUtils.IpMatchesOrIsWithinCidrRange(ip, ruleIP)) == true;
}
