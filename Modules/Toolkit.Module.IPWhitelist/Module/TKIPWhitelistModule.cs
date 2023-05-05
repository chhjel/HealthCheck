using QoDL.Toolkit.Core.Abstractions.Modules;
using QoDL.Toolkit.Core.Config;
using QoDL.Toolkit.Core.Util;
using QoDL.Toolkit.Module.IPWhitelist.Models;
using QoDL.Toolkit.Web.Core.Utils;
using QoDL.Toolkit.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using QoDL.Toolkit.Module.IPWhitelist.Utils;

#if NETCORE
using Microsoft.AspNetCore.Http;
#endif

namespace QoDL.Toolkit.Module.IPWhitelist.Module;

/// <summary>
/// Allows for blocking everything except for some whitelisted ips.
/// </summary>
public class TKIPWhitelistModule : ToolkitModuleBase<TKIPWhitelistModule.AccessOption>
{
    private TKIPWhitelistModuleOptions Options { get; }

    /// <summary>
    /// Allows for blocking everything except for some whitelisted ips.
    /// </summary>
    public TKIPWhitelistModule(TKIPWhitelistModuleOptions options)
    {
        Options = options;
    }

    /// <summary>
    /// Check options object for issues.
    /// </summary>
    public override IEnumerable<string> Validate()
    {
        var issues = new List<string>();
        if (Options.Service == null) issues.Add("Options.Service must be set.");
        if (Options.RuleStorage == null) issues.Add("Options.RuleStorage must be set.");
        if (Options.ConfigStorage == null) issues.Add("Options.ConfigStorage must be set.");
        if (Options.IPStorage == null) issues.Add("Options.IPStorage must be set.");
        return issues;
    }

    /// <summary>
    /// Get frontend options for this module.
    /// </summary>
    public override object GetFrontendOptionsObject(ToolkitModuleContext context) => null;

    /// <summary>
    /// Get config for this module.
    /// </summary>
    public override IToolkitModuleConfig GetModuleConfig(ToolkitModuleContext context) => new TKIPWhitelistModuleConfig();

    /// <summary>
    /// Different access options for this module.
    /// </summary>
    [Flags]
    public enum AccessOption
    {
        /// <summary>Does nothing.</summary>
        None = 0
    }

    #region Invokable methods
    /// <summary></summary>
    [ToolkitModuleMethod]
    public async Task<TKIPWhitelistConfig> GetConfig()
        => (await Options.ConfigStorage.GetConfigAsync()) ?? new();

    /// <summary></summary>
    [ToolkitModuleMethod]
    public async Task SaveConfig(ToolkitModuleContext context, TKIPWhitelistConfig config)
    {
        await Options.ConfigStorage.SaveConfigAsync(config);

        // Store audit data
        context.AddAuditEvent("Save IP whitelist config")
            .AddClientConnectionDetails(context);
    }

    /// <summary></summary>
    [ToolkitModuleMethod]
    public IEnumerable<TKIPWhitelistLogItem> GetLog()
        => Options.Service.GetLog();

    /// <summary></summary>
    [ToolkitModuleMethod]
    public bool ClearRequestLog()
    {
        Options.Service.ClearLogs();
        return true;
    }

    /// <summary></summary>
    [ToolkitModuleMethod]
    public async Task<IEnumerable<TKIPWhitelistRule>> GetRules()
        => await Options.RuleStorage.GetRulesAsync();

    /// <summary></summary>
    [ToolkitModuleMethod]
    public async Task<TKIPWhitelistRule> SaveRule(ToolkitModuleContext context, TKIPWhitelistRule rule)
    {
        rule = await Options.RuleStorage.StoreRuleAsync(rule);

        // Store audit data
        context.AddAuditEvent("Save IP whitelist rule", rule.Name)
            .AddClientConnectionDetails(context)
            .AddDetail("Rule Id", rule.Id.ToString());

        return rule;
    }

    /// <summary></summary>
    [ToolkitModuleMethod]
    public async Task DeleteRule(ToolkitModuleContext context, Guid id)
    {
        await Options.IPStorage.DeleteRuleIPsAsync(id);
        await Options.LinkStorage.DeleteRuleLinksAsync(id);
        await Options.RuleStorage.DeleteRuleAsync(id);
        await Options.Service.InvalidateIPCacheAsync();

        // Store audit data
        context.AddAuditEvent("Delete IP whitelist rule", id.ToString())
            .AddClientConnectionDetails(context);
    }

    /// <summary></summary>
    [ToolkitModuleMethod]
    public bool IpMatchesCidr(TKIPWhitelistCidrTest payload)
    {
        try
        {
            return TKIPAddressUtils.IpMatchesOrIsWithinCidrRange(payload.IP, payload.IPWithOptionalCidr);
        }
        catch(Exception)
        {
            return false;
        }
    }

    /// <summary></summary>
    [ToolkitModuleMethod]
    public async Task<TKIPWhitelistCheckResult> IsRequestAllowed(TKIPWhitelistTestRequest payload)
    {
        var pathOnly = payload.Path;
        if (pathOnly.Contains("?")) pathOnly = pathOnly.Split('?')[0];

        var data = new TKIPWhitelistRequestData {
            IP = payload.RawIP,
            PathAndQuery = payload.Path,
            Path = pathOnly,
#if NETCORE
            Context = (TKGlobalConfig.GetDefaultInstanceResolver()?.Invoke(typeof(IHttpContextAccessor)) as IHttpContextAccessor)?.HttpContext
#endif
#if NETFULL
            Request = HttpContext.Current.Request
#endif
        };
        return await Options.Service.IsRequestAllowedAsync(data, testMode: true);
    }

    /// <summary></summary>
    [ToolkitModuleMethod]
    public async Task DeleteRuleLink(ToolkitModuleContext context, Guid id)
    {
        await Options.LinkStorage.DeleteRuleLinkAsync(id);

        // Store audit data
        context.AddAuditEvent("Delete IP whitelist rule link", id.ToString())
            .AddClientConnectionDetails(context);
    }

    /// <summary></summary>
    [ToolkitModuleMethod]
    public async Task DeleteRuleIP(ToolkitModuleContext context, Guid id)
    {
        await Options.IPStorage.DeleteRuleIPAsync(id);
        await Options.Service.InvalidateIPCacheAsync();

        // Store audit data
        context.AddAuditEvent("Delete IP whitelist IP", id.ToString())
            .AddClientConnectionDetails(context);
    }

    /// <summary></summary>
    [ToolkitModuleMethod]
    public async Task<IEnumerable<TKIPWhitelistLink>> GetRuleLinks(Guid id)
        => await Options.LinkStorage.GetRuleLinksAsync(id);

    /// <summary></summary>
    [ToolkitModuleMethod]
    public async Task<IEnumerable<TKIPWhitelistIP>> GetRuleIPs(Guid id)
        => await Options.IPStorage.GetRuleIPsAsync(id);

    /// <summary></summary>
    [ToolkitModuleMethod]
    public async Task<TKIPWhitelistLink> StoreRuleLink(ToolkitModuleContext context, TKIPWhitelistLink link)
    {
        // Store audit data
        context.AddAuditEvent("Create IP whitelist rule link", link.Name)
            .AddClientConnectionDetails(context)
            .AddDetail("Rule Id", link.Id.ToString());

        return await Options.LinkStorage.StoreRuleLinkAsync(link);
    }

    /// <summary></summary>
    [ToolkitModuleMethod]
    public async Task<TKIPWhitelistIP> StoreRuleIP(ToolkitModuleContext context, TKIPWhitelistIP ip)
    {
        ip.Note = $"Added by '{context.UserName}'";

        // Store audit data
        context.AddAuditEvent("Add IP whitelist IP", ip.IP)
            .AddClientConnectionDetails(context)
            .AddDetail("IP Id", ip.Id.ToString());

        var updatedIp = await Options.IPStorage.StoreRuleIPAsync(ip);
        await Options.Service.InvalidateIPCacheAsync();
        return updatedIp;
    }

    /// <summary></summary>
    [ToolkitModuleMethod]
    public async Task<List<TKIPWhitelistIP>> StoreRuleIPs(ToolkitModuleContext context, List<TKIPWhitelistIP> ips)
    {
        var updatedIps = new List<TKIPWhitelistIP>();
        if (!ips.Any()) return updatedIps;

        var rule = await Options.RuleStorage.GetRuleAsync(ips[0].RuleId);
        var existingIps = await Options.IPStorage.GetRuleIPsAsync(rule.Id);

        foreach (var ip in ips)
        {
            var exists = existingIps.Any(x => x.IP.Equals(ip.IP, StringComparison.InvariantCultureIgnoreCase));
            if (exists) continue;

            ip.Note = $"Added by '{context.UserName}'";

            // Store audit data
            context.AddAuditEvent("Add IP whitelist IP", ip.IP)
                .AddClientConnectionDetails(context)
                .AddDetail("IP Id", ip.Id.ToString());

            updatedIps.Add(await Options.IPStorage.StoreRuleIPAsync(ip));
        }

        await Options.Service.InvalidateIPCacheAsync();
        return updatedIps;
    }
    #endregion

    #region Actions
    /// <summary>
    /// Whitelist based on a generated link - page display.
    /// </summary>
    [ToolkitModuleAction]
    public async Task<object> IPWLLink(ToolkitModuleContext context, string url)
    {
        (Guid? ruleId, string secret) = ParseRuleIdSecretFromActionUrl(url);
        if (ruleId == null) return null;
        
        var link = await Options.LinkStorage.GetRuleLinkFromSecretAsync(ruleId.Value, secret);
        if (link == null || link.InvitationExpiresAt < DateTimeOffset.Now) return null;

        //// Store audit data
        //context.AddAuditEvent("File download", definition.FileName)
        //    .AddClientConnectionDetails(context)
        //    .AddDetail("File Name", definition.FileName)
        //    .AddDetail("File Id", definition.FileId)
        //    .AddDetail("Storage Id", definition.StorageId);
        return CreateWhiteListLinkPageHtml(context, link);
    }

    /// <summary>
    /// Whitelist based on a generated link - whitelist action.
    /// </summary>
    [ToolkitModuleAction]
    public async Task<object> IPWLLinkActivate(ToolkitModuleContext context, string url)
    {
        if (context?.Request?.IsPOST != true) return null;

        var ipFromHeader = context.Request.Headers["x-add-ip"]?.Trim();
        if (string.IsNullOrWhiteSpace(ipFromHeader)) return null;
        var ips = ipFromHeader.Split('_').Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
        if (ips.Count == 0) return null;

        (Guid? ruleId, string secret) = ParseRuleIdSecretFromActionUrl(url);
        if (ruleId == null) return null;

        var link = await Options.LinkStorage.GetRuleLinkFromSecretAsync(ruleId.Value, secret);
        if (link == null || link.InvitationExpiresAt < DateTimeOffset.Now) return createResult(false, "Link expired");

        var rule = await Options.RuleStorage.GetRuleAsync(link.RuleId);
        if (rule == null) return createResult(false, "Matching whitelist rule not found.");
        else if (!rule.Enabled) return createResult(false, "Matching whitelist rule found but is disabled.");
        else if (rule.EnabledUntil < DateTimeOffset.Now) return createResult(false, "Matching whitelist rule found but is disabled (enabled-until expired).");

        var addedCount = 0;
        var existingCount = 0;
        var ruleIps = await Options.IPStorage.GetRuleIPsAsync(rule.Id);
        foreach(var ip in ips)
        {
            var alreadyExists = ruleIps.Any(x => x.IP.Equals(ip, StringComparison.CurrentCultureIgnoreCase));
            if (alreadyExists)
            {
                existingCount++;
                continue;
            }

            await Options.IPStorage.StoreRuleIPAsync(new TKIPWhitelistIP { IP = ip, Note = $"Added from link '{link.Name}'.", RuleId = rule.Id });
            addedCount++;
        }

        if (addedCount > 0)
        {
            await Options.Service.InvalidateIPCacheAsync();
        }

        // Store audit data
        context.AddAuditEvent("IP whitelisted using link", link.Name)
            .AddClientConnectionDetails(context)
            .AddDetail("Rule id", link.RuleId.ToString())
            .AddDetail("Link id", link.Id.ToString());

        if (addedCount > 0 && existingCount > 0) return createResult(true, $"Whitelisted {addedCount} new IPs, {existingCount} already whitelisted.");
        else if (addedCount > 0) return createResult(true, $"Whitelisted {addedCount} new IPs");
        else if (existingCount > 0) return createResult(true, $"No new IPs whitelisted, {existingCount} IPs already whitelisted.");
        else return createResult(true, $"No new IPs added");

        static string createResult(bool success, string note)
        {
            return
                "{\n" +
                $"  \"success\": {success.ToString().ToLower()},\n" +
                $"  \"note\": {EscapeJsString(note)}\n" +
                "}";
        }
    }

    private static (Guid? ruleId, string secret) ParseRuleIdSecretFromActionUrl(string url)
    {
        var urlPath = url;
        if (urlPath?.Contains('?') == true) urlPath = urlPath.Split('?')[0].Trim();

        var lastSegment = urlPath?.Split('/')?.Where(x => !string.IsNullOrWhiteSpace(x))?.LastOrDefault() ?? string.Empty;
        var urlSegments = lastSegment.Split(new[] { '_' }, 2).Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
        if (urlSegments.Count < 2) return (null, null);

        if (!Guid.TryParse(urlSegments[urlSegments.Count - 2], out var ruleId)) return (null, null);

        var secret = urlSegments[urlSegments.Count - 1];
        if (string.IsNullOrWhiteSpace(secret)) return (null, null);

        return (ruleId, secret);
    }

    private static string EscapeJsString(string value, bool addQuotes = true)
        => (value == null) ? "null" : HttpUtility.JavaScriptStringEncode(value, addQuotes);

    /// <summary>
    /// Create the html to show for the download file page when not downloading directly.
    /// </summary>
    protected virtual string CreateWhiteListLinkPageHtml(ToolkitModuleContext context, TKIPWhitelistLink link)
    {
        var title = Options.WhitelistLinkPageTitle ?? "";

        var cssTagsHtml = TKAssetGlobalConfig.CreateCssTags(context.CssUrls);
        var jsTagsHtml = TKAssetGlobalConfig.CreateJavaScriptTags(context.JavaScriptUrls);
        var currentIp = string.Empty;

        try
        {
#if NETFULL
            if (HttpContext.Current?.Request != null) currentIp = TKRequestUtils.GetIPAddress(new HttpContextWrapper(HttpContext.Current).Request);
#elif NETCORE
            var accessor = TKGlobalConfig.GetDefaultInstanceResolver()?.Invoke(typeof(IHttpContextAccessor)) as IHttpContextAccessor;
            if (accessor?.HttpContext != null) currentIp = TKRequestUtils.GetIPAddress(accessor.HttpContext);
#endif
        }
        catch (Exception) { }

        return $@"
<!doctype html>
<html>
<head>
    <title>{title}</title>
    <meta charset=""utf-8"">
    <meta name=""robots"" content=""noindex"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1, maximum-scale=1, user-scalable=no, minimal-ui"">
    {cssTagsHtml}
</head>

<body>
    <div id=""ipwl-link""></div>

    <script>
        window.__ipwl_data = {{
            currentIp: {EscapeJsString(currentIp)},
            ruleId: {EscapeJsString(link.RuleId.ToString())},
            secret: {EscapeJsString(link.Secret)},
            note: {EscapeJsString(link.Note)}
        }};
    </script>
    {jsTagsHtml}
</body>
</html>";
    }
    #endregion
}
