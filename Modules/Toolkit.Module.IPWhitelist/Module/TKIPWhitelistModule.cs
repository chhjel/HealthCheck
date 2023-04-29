using QoDL.Toolkit.Core.Abstractions.Modules;
using QoDL.Toolkit.Core.Config;
using QoDL.Toolkit.Module.IPWhitelist.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web;

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
        => await Options.ConfigStorage.GetConfigAsync();

    /// <summary></summary>
    [ToolkitModuleMethod]
    public async Task SaveConfig(ToolkitModuleContext context, TKIPWhitelistConfig config)
    {
        await Options.ConfigStorage.SaveConfigAsync(config);

        // Store audit data
        context.AddAuditEvent("Save config")
            .AddClientConnectionDetails(context);
    }

    /// <summary></summary>
    [ToolkitModuleMethod]
    public IEnumerable<TKIPWhitelistLogItem> GetLog()
        => Options.Service.GetLog();

    /// <summary></summary>
    [ToolkitModuleMethod]
    public async Task<IEnumerable<TKIPWhitelistRule>> GetRules()
        => await Options.RuleStorage.GetRulesAsync();

    /// <summary></summary>
    [ToolkitModuleMethod]
    public async Task SaveRule(ToolkitModuleContext context, TKIPWhitelistRule rule)
    {
        await Options.RuleStorage.StoreRuleAsync(rule);

        // Store audit data
        context.AddAuditEvent("Save rule", rule.Name)
            .AddClientConnectionDetails(context)
            .AddDetail("Rule Id", rule.Id.ToString());
    }

    /// <summary></summary>
    [ToolkitModuleMethod]
    public async Task DeleteRule(ToolkitModuleContext context, Guid id)
    {
        await Options.RuleStorage.DeleteRuleAsync(id);

        // Store audit data
        context.AddAuditEvent("Delete rule", id.ToString())
            .AddClientConnectionDetails(context);
    }
    #endregion

    #region Actions
    /// <summary>
    /// Whitelist based on a generated link.
    /// </summary>
    [ToolkitModuleAction]
    public object IPWLLink(ToolkitModuleContext context, string url)
    {
        //return null;

        //// Store audit data
        //context.AddAuditEvent("File download", definition.FileName)
        //    .AddClientConnectionDetails(context)
        //    .AddDetail("File Name", definition.FileName)
        //    .AddDetail("File Id", definition.FileId)
        //    .AddDetail("Storage Id", definition.StorageId);
        return CreateWhiteListLinkPageHtml(context);
    }

    /// <summary>
    /// Whitelist based on a generated link.
    /// </summary>
    [ToolkitModuleAction]
    public object IPWLLinkActivate(ToolkitModuleContext context)
    {
        //// Store audit data
        //context.AddAuditEvent("File download", definition.FileName)
        //    .AddClientConnectionDetails(context)
        //    .AddDetail("File Name", definition.FileName)
        //    .AddDetail("File Id", definition.FileId)
        //    .AddDetail("Storage Id", definition.StorageId);
        return "OK";
    }

    private static string EscapeJsString(string value, bool addQuotes = true)
        => (value == null) ? "null" : HttpUtility.JavaScriptStringEncode(value, addQuotes);

    /// <summary>
    /// Create the html to show for the download file page when not downloading directly.
    /// </summary>
    protected virtual string CreateWhiteListLinkPageHtml(ToolkitModuleContext context)
    {
        var downloadLink = "asd";

        var title = Options.WhitelistLinkPageTitle ?? "";

        var cssTagsHtml = TKAssetGlobalConfig.CreateCssTags(context.CssUrls);
        var jsTagsHtml = TKAssetGlobalConfig.CreateJavaScriptTags(context.JavaScriptUrls);

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
    <div id=""app-download""></div>

    <script>
        window.__ipwl_data = {{
            definitionValidationError: {EscapeJsString("")},
            download: {{
                name: {EscapeJsString("")},
                filename: {EscapeJsString("")},
                downloadLink: {EscapeJsString(downloadLink)}
            }}
        }};
    </script>
    {jsTagsHtml}
</body>
</html>";
    }
    #endregion
}