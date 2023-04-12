using QoDL.Toolkit.Core.Abstractions.Modules;
using QoDL.Toolkit.Core.Modules.Dataflow.Abstractions;
using QoDL.Toolkit.Core.Modules.Dataflow.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QoDL.Toolkit.Core.Modules.Dataflow;

/// <summary>
/// Module for viewing custom data.
/// </summary>
public class TKDataflowModule<TAccessRole> : ToolkitModuleBase<TKDataflowModule<TAccessRole>.AccessOption>
{
    private TKDataflowModuleOptions<TAccessRole> Options { get; }

    /// <summary>
    /// Module for viewing custom data.
    /// </summary>
    public TKDataflowModule(TKDataflowModuleOptions<TAccessRole> options)
    {
        Options = options;
    }

    /// <summary>
    /// Check options object for issues.
    /// </summary>
    public override IEnumerable<string> Validate()
    {
        var issues = new List<string>();
        if (Options.DataflowService == null) issues.Add("Options.DataflowService must be set.");
        return issues;
    }

    /// <summary>
    /// Get frontend options for this module.
    /// </summary>
    public override object GetFrontendOptionsObject(ToolkitModuleContext context) => null;

    /// <summary>
    /// Get config for this module.
    /// </summary>
    public override IToolkitModuleConfig GetModuleConfig(ToolkitModuleContext context) => new TKDataflowModuleConfig();

    /// <summary>
    /// Different access options for this module.
    /// </summary>
    [Flags]
    public enum AccessOption
    {
        /// <summary>Does nothing.</summary>
        None = 0,
        /// <summary>Allows fetching data from streams.</summary>
        FetchStream = 1,
        /// <summary>Allows performing unified searches.</summary>
        UnifiedSearch = 2
    }

    #region Invokable methods
    /// <summary>
    /// Get viewmodel for dataflow entries result.
    /// </summary>
    [ToolkitModuleMethod]
    public async Task<IEnumerable<IDataflowEntry>> GetDataflowStreamEntries(ToolkitModuleContext context, GetDataflowStreamEntriesFilter model)
    {
        if (Options.DataflowService == null || !context.HasAccess(AccessOption.FetchStream))
            return Enumerable.Empty<IDataflowEntry>();

        var metadatas = GetDataflowStreamsMetadata(context);
        if (!metadatas.Any())
            return Enumerable.Empty<IDataflowEntry>();

        var stream = metadatas.FirstOrDefault(x => x.Id == model.StreamId);
        if (stream != null)
        {
            context.AddAuditEvent(action: "Dataflow stream fetched", subject: stream?.Name)
                .AddDetail("Stream id", stream?.Id);
        }

        model.StreamFilter ??= new DataflowStreamFilter();
        model.StreamFilter.PropertyFilters ??= new Dictionary<string, string>();
        return await Options.DataflowService.GetEntries(model.StreamId, model.StreamFilter);
    }

    /// <summary>
    /// Get viewmodel for dataflow streams metadata result.
    /// </summary>
    [ToolkitModuleMethod]
    public IEnumerable<DataflowStreamMetadata<TAccessRole>> GetDataflowStreamsMetadata(ToolkitModuleContext context)
    {
        if (!context.HasAccess(AccessOption.FetchStream))
            return Enumerable.Empty<DataflowStreamMetadata<TAccessRole>>();

        return Options.DataflowService.GetStreamMetadata()
            .Where(x => context.HasRoleAccess(x.RolesWithAccess, defaultValue: true));
    }

    /// <summary>
    /// Get viewmodel for dataflow unified search metadatas.
    /// </summary>
    [ToolkitModuleMethod]
    public IEnumerable<DataflowUnifiedSearchMetadata<TAccessRole>> GetDataflowUnifiedSearchMetadata(ToolkitModuleContext context)
    {
        if (!context.HasAccess(AccessOption.UnifiedSearch))
            return Enumerable.Empty<DataflowUnifiedSearchMetadata<TAccessRole>>();

        return Options.DataflowService.GetUnifiedSearchesMetadata()
            .Where(x => context.HasRoleAccess(x.RolesWithAccess, defaultValue: true));
    }

    /// <summary>
    /// Get viewmodel for dataflow entries result.
    /// </summary>
    [ToolkitModuleMethod]
    public async Task<TKDataflowUnifiedSearchResult> UnifiedSearch(ToolkitModuleContext context, TKDataFlowUnifiedSearchRequest model)
    {
        if (Options.DataflowService == null || !context.HasAccess(AccessOption.UnifiedSearch))
            return new TKDataflowUnifiedSearchResult();

        var metadatas = GetDataflowUnifiedSearchMetadata(context);
        if (!metadatas.Any())
            return new TKDataflowUnifiedSearchResult();

        var search = metadatas.FirstOrDefault(x => x.Id == model.SearchId);
        if (search != null)
        {
            context.AddAuditEvent(action: "Dataflow search fetched", subject: search?.Name)
                .AddDetail("Query", model.Query);
        }

        return await Options.DataflowService.UnifiedSearchAsync(model.SearchId, model.Query, model.PageIndex, model.PageSize);
    }
    #endregion
}
