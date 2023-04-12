using QoDL.Toolkit.Core.Abstractions.Modules;
using QoDL.Toolkit.Core.Modules.ReleaseNotes.Models;
using QoDL.Toolkit.Core.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QoDL.Toolkit.Core.Modules.ReleaseNotes;

/// <summary>
/// Module for viewing release notes.
/// </summary>
public class TKReleaseNotesModule : ToolkitModuleBase<TKReleaseNotesModule.AccessOption>
{
    private TKReleaseNotesModuleOptions Options { get; }

    /// <summary>
    /// Module for viewing ReleaseNotes.
    /// </summary>
    public TKReleaseNotesModule(TKReleaseNotesModuleOptions options)
    {
        Options = options;
    }

    /// <summary>
    /// Check options object for issues.
    /// </summary>
    public override IEnumerable<string> Validate()
    {
        var issues = new List<string>();
        if (Options.ReleaseNotesProvider == null) issues.Add("Options.ReleaseNotesProvider must be set.");
        return issues;
    }

    /// <summary>
    /// Get frontend options for this module.
    /// </summary>
    public override object GetFrontendOptionsObject(ToolkitModuleContext context) => new TKReleaseNotesFrontendOptions
    {
        TopLinks = GetAllowedTopLinks(context) ?? new()
    };

    /// <summary>
    /// Get config for this module.
    /// </summary>
    public override IToolkitModuleConfig GetModuleConfig(ToolkitModuleContext context) => new TKReleaseNotesModuleConfig();

    /// <summary>
    /// Different access options for this module.
    /// </summary>
    [Flags]
    public enum AccessOption
    {
        /// <summary>Does nothing.</summary>
        None = 0,

        /// <summary>Includes git commit messages, changes without issue ids, author and PR links.</summary>
        DeveloperDetails = 1,
    }

    #region Invokable methods
    /// <summary></summary>
    [ToolkitModuleMethod]
    public async Task<TKReleaseNotesViewModel> GetReleaseNotesWithoutDevDetails()
    {
        var model = await Options.ReleaseNotesProvider.GetViewModelAsync();
        return model?.WithoutDevDetails ?? new TKReleaseNotesViewModel { ErrorMessage = "No release notes found." };
    }

    /// <summary></summary>
    [ToolkitModuleMethod(AccessOption.DeveloperDetails)]
    public async Task<TKReleaseNotesViewModel> GetReleaseNotesWithDevDetails()
    {
        var model = await Options.ReleaseNotesProvider.GetViewModelAsync();
        return model?.WithDevDetails ?? new TKReleaseNotesViewModel { ErrorMessage = "No release notes found." };
    }
    #endregion

    private List<TKReleaseNoteLinkWithAccess> GetAllowedTopLinks(ToolkitModuleContext context)
    {
        if (Options.TopLinks?.Any() != true) return Options.TopLinks;

        return Options.TopLinks
            .Where(x => x.AccessRequired == null || context.HasAnyOfRoles(x.AccessRequired))
            .ToList();
    }
}
