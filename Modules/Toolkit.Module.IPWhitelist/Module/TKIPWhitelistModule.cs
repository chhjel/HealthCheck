using QoDL.Toolkit.Core.Abstractions.Modules;
using System;
using System.Collections.Generic;

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
    // /// <summary>
    // /// </summary>
    // [ToolkitModuleMethod]
    // public Task<object> GetSomething(/*ToolkitModuleContext context*/)
    // {
    //     return Task.FromResult(data);
    // }
    #endregion
}
