using QoDL.Toolkit.Core.Abstractions.Modules;
using QoDL.Toolkit.Core.Config;
using QoDL.Toolkit.Core.Modules.SecureFileDownload.Models;
using QoDL.Toolkit.Core.Util.Modules;
using QoDL.Toolkit.Module.IPWhitelist.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web;

namespace QoDL.Toolkit.Module.IPWhitelist.Module;

/// <summary>
/// 
/// </summary>
public class TKIPWhitelistModule : ToolkitModuleBase<TKIPWhitelistModule.AccessOption>
{
    private TKIPWhitelistModuleOptions Options { get; }

    /// <summary>
    /// 
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
    public async Task<TKIPWhitelistConfig> GetConfig(/*ToolkitModuleContext context*/)
        => await Options.ConfigStorage.GetConfigAsync();

    /// <summary></summary>
    [ToolkitModuleMethod]
    public async Task SaveConfig(TKIPWhitelistConfig config)
        => await Options.ConfigStorage.SaveConfigAsync(config);

    /// <summary></summary>
    [ToolkitModuleMethod]
    public async Task<IEnumerable<TKIPWhitelistRule>> GetRules()
        => await Options.RuleStorage.GetRulesAsync();

    /// <summary></summary>
    [ToolkitModuleMethod]
    public async Task SaveRule(TKIPWhitelistRule rule)
        => await Options.RuleStorage.StoreRuleAsync(rule);

    /// <summary></summary>
    [ToolkitModuleMethod]
    public async Task DeleteRule(Guid id)
        => await Options.RuleStorage.DeleteRuleAsync(id);
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
