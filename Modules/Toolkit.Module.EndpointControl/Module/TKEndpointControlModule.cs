using QoDL.Toolkit.Core.Abstractions.Modules;
using QoDL.Toolkit.Core.Attributes;
using QoDL.Toolkit.Module.EndpointControl.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace QoDL.Toolkit.Module.EndpointControl.Module;

/// <summary>
/// Module for controlling specified endpoints.
/// </summary>
public class TKEndpointControlModule : ToolkitModuleBase<TKEndpointControlModule.AccessOption>
{
    private TKEndpointControlModuleOptions Options { get; }

    /// <summary>
    /// Module for controlling specified endpoints.
    /// </summary>
    public TKEndpointControlModule(TKEndpointControlModuleOptions options)
    {
        Options = options;
    }

    /// <summary>
    /// Check options object for issues.
    /// </summary>
    public override IEnumerable<string> Validate()
    {
        var issues = new List<string>();
        if (Options.EndpointControlService == null) issues.Add("Options.EndpointControlService must be set.");
        if (Options.RuleStorage == null) issues.Add("Options.RuleStorage must be set.");
        if (Options.DefinitionStorage == null) issues.Add("Options.DefinitionStorage must be set.");
        if (Options.HistoryStorage == null) issues.Add("Options.HistoryStorage must be set.");
        return issues;
    }

    /// <summary>
    /// Get frontend options for this module.
    /// </summary>
    public override object GetFrontendOptionsObject(ToolkitModuleContext context) => new
    {
        MaxLatestRequestsToShow = Options.MaxLatestRequestsToShow,
        MaxLatestSimpleRequestDataToShow = Options.MaxLatestSimpleRequestDataToShow
    };

    /// <summary>
    /// Get config for this module.
    /// </summary>
    public override IToolkitModuleConfig GetModuleConfig(ToolkitModuleContext context) => new TKEndpointControlModuleConfig();

    /// <summary>
    /// Different access options for this module.
    /// </summary>
    [Flags]
    public enum AccessOption
    {
        /// <summary>Does nothing.</summary>
        None = 0,

        /// <summary>
        /// Allows for deleting/clearing endpoint definitions.
        /// </summary>
        EditEndpointDefinitions = 1,

        /// <summary>
        /// Allows for viewing latest incoming request details.
        /// </summary>
        ViewLatestRequestData = 2,

        /// <summary>
        /// Allows for viewing latest incoming request charts.
        /// </summary>
        ViewRequestCharts = 4,

        /// <summary>
        /// Allows for disabling/enabling/editing/deleting rules.
        /// </summary>
        EditRules = 8
    }

    #region Invokable
    /// <summary>
    /// Get all the stored rules.
    /// </summary>
    [ToolkitModuleMethod]
    public EndpointControlDataViewModel GetData()
    {
        var customResults = Options.EndpointControlService.GetCustomBlockedResults()
            ?.Select(x => new EndpointControlCustomResultDefinitionViewModel
            {
                Id = x.Id,
                Name = x.Name,
                Description = x.Description,
                CustomProperties = TKCustomPropertyAttribute.CreateInputConfigs(x.CustomPropertiesModelType)
            })
            ?.ToList();

        var conditions = Options.EndpointControlService.GetConditions()
            ?.Select(x => new TKEndpointControlConditionDefinitionViewModel
            {
                Id = x.GetType().Name,
                Name = x.Name,
                Description = x.Description,
                CustomProperties = TKCustomPropertyAttribute.CreateInputConfigs(x.CustomPropertiesModelType)
            })
            ?.ToList();

        return new EndpointControlDataViewModel
        {
            Rules = Options.RuleStorage.GetRules(),
            EndpointDefinitions = Options.DefinitionStorage.GetDefinitions(),
            CustomResultDefinitions = customResults,
            Conditions = conditions
        };
    }

    /// <summary>
    /// Enable/disable rule with the given id.
    /// </summary>
    [ToolkitModuleMethod(AccessOption.EditRules)]
    public object SetRuleEnabled(ToolkitModuleContext context, SetRuleEnabledRequestModel model)
    {
        var rule = Options.RuleStorage.GetRule(model.RuleId);
        if (rule == null)
            return new { Success = false };

        rule.Enabled = model.Enabled;
        rule.LastChangedBy = context?.UserName ?? "Anonymous";
        rule.LastChangedAt = DateTimeOffset.Now;

        rule = Options.RuleStorage.UpdateRule(rule);

        context?.AddAuditEvent($"{(model.Enabled ? "Enabled" : "Disabled")} endpoint control rule", rule.Id.ToString());
        return new { Success = true };
    }

    /// <summary>
    /// Delete a rule with the given id.
    /// </summary>
    [ToolkitModuleMethod(AccessOption.EditRules)]
    public object DeleteRule(ToolkitModuleContext context, Guid id)
    {
        var rule = Options.RuleStorage.GetRule(id);
        if (rule == null)
            return new { Success = false };

        Options.RuleStorage.DeleteRule(id);
        context?.AddAuditEvent($"Deleted endpoint control rule", rule.Id.ToString());

        return new { Success = true };
    }

    /// <summary>
    /// Create a new or update an existing rule.
    /// </summary>
    [ToolkitModuleMethod(AccessOption.EditRules)]
    public EndpointControlRule CreateOrUpdateRule(ToolkitModuleContext context, EndpointControlRule rule)
    {
        rule.LastChangedBy = context?.UserName ?? "Anonymous";
        rule.LastChangedAt = DateTimeOffset.Now;

        var isNew = Options.RuleStorage.GetRule(rule.Id) == null;
        rule = (isNew)
            ? Options.RuleStorage.InsertRule(rule)
            : Options.RuleStorage.UpdateRule(rule);

        context?.AddAuditEvent($"{(isNew ? "Created" : "Updated")} endpoint control rule", rule.Id.ToString());
        return rule;
    }

    /// <summary>
    /// Delete a definition with the given id.
    /// </summary>
    [ToolkitModuleMethod(AccessOption.EditEndpointDefinitions)]
    public object DeleteDefinition(ToolkitModuleContext context, string endpointId)
    {
        if (!Options.DefinitionStorage.HasDefinitionFor(endpointId))
            return new { Success = false };

        Options.DefinitionStorage.DeleteDefinition(endpointId);
        context?.AddAuditEvent($"Deleted endpoint definition", endpointId);

        return new { Success = true };
    }

    /// <summary>
    /// Delete all definitions.
    /// </summary>
    [ToolkitModuleMethod(AccessOption.EditEndpointDefinitions)]
    public object DeleteAllDefinitions(ToolkitModuleContext context)
    {
        Options.DefinitionStorage.ClearAllDefinitions();
        context?.AddAuditEvent($"Deleted all endpoint definitions");
        return new { Success = true };
    }

    /// <summary>
    /// Get latest requests.
    /// </summary>
    [ToolkitModuleMethod(AccessOption.ViewLatestRequestData)]
    public IEnumerable<EndpointRequestDetails> GetLatestRequests()
        => Options.HistoryStorage.GetLatestRequests(maxCount: Options.MaxLatestRequestsToShow);

    /// <summary>
    /// Get latest requests.
    /// </summary>
    [ToolkitModuleMethod(AccessOption.ViewRequestCharts)]
    public IEnumerable<EndpointRequestSimpleDetails> GetLatestRequestsSimple()
        => Options.HistoryStorage
            .GetLatestRequests(maxCount: Options.MaxLatestSimpleRequestDataToShow)
            .Select(x => new EndpointRequestSimpleDetails
            {
                EndpointId = x.EndpointId,
                Timestamp = x.Timestamp,
                WasBlocked = x.WasBlocked
            });
    #endregion
}
