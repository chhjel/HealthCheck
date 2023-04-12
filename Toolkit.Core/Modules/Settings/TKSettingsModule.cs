using QoDL.Toolkit.Core.Abstractions.Modules;
using QoDL.Toolkit.Core.Attributes;
using QoDL.Toolkit.Core.Modules.Settings.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace QoDL.Toolkit.Core.Modules.Settings;

/// <summary>
/// Module for configuring custom settings.
/// </summary>
public class TKSettingsModule : ToolkitModuleBase<TKSettingsModule.AccessOption>
{
    private TKSettingsModuleOptions Options { get; }

    /// <summary>
    /// Module for configuring custom settings.
    /// </summary>
    public TKSettingsModule(TKSettingsModuleOptions options)
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
        if (Options.ModelType == null) issues.Add("Options.ModelType must be set.");
        return issues;
    }

    /// <summary>
    /// Get frontend options for this module.
    /// </summary>
    public override object GetFrontendOptionsObject(ToolkitModuleContext context) => null;

    /// <summary>
    /// Get config for this module.
    /// </summary>
    public override IToolkitModuleConfig GetModuleConfig(ToolkitModuleContext context) => new TKSettingsModuleConfig();

    /// <summary>
    /// Different access options for this module.
    /// </summary>
    [Flags]
    public enum AccessOption
    {
        /// <summary>Does nothing.</summary>
        None = 0,

        /// <summary>Allowed to change settings.</summary>
        ChangeSettings = 1
    }

    #region Invokable methods
    /// <summary>
    /// Get settings.
    /// </summary>
    [ToolkitModuleMethod]
    public GetSettingsViewModel GetSettings()
    {
        var values = Options.Service.GetSettingValues(Options.ModelType);
        return new GetSettingsViewModel()
        {
            Definitions = TKCustomPropertyAttribute.CreateInputConfigs(Options.ModelType),
            Values = values
        };
    }

    /// <summary>
    /// Save the given settings.
    /// </summary>
    [ToolkitModuleMethod(AccessOption.ChangeSettings)]
    public void SetSettings(ToolkitModuleContext context, SetSettingsViewModel model)
    {
        string settingsString = $"[{string.Join("] [", model.Values.Select(x => $"{x.Key}: {x.Value}"))}]";
        context.AddAuditEvent(action: "Settings updated", subject: "Settings")
            .AddDetail("Values", settingsString);

        Options.Service.SaveSettings(Options.ModelType, model.Values);
    }
    #endregion
}
