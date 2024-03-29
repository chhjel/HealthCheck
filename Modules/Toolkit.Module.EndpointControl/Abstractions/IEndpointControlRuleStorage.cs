using QoDL.Toolkit.Module.EndpointControl.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace QoDL.Toolkit.Module.EndpointControl.Abstractions;

/// <summary>
/// Provides <see cref="EndpointControlRule"/>s.
/// </summary>
public interface IEndpointControlRuleStorage
{
    /// <summary>
    /// Get a single rule by id.
    /// </summary>
    EndpointControlRule GetRule(Guid id);

    /// <summary>
    /// Get all rules.
    /// </summary>
    IEnumerable<EndpointControlRule> GetRules();

    /// <summary>
    /// Insert the given rule.
    /// </summary>
    EndpointControlRule InsertRule(EndpointControlRule rule);

    /// <summary>
    /// Updates the given rule.
    /// </summary>
    EndpointControlRule UpdateRule(EndpointControlRule rule);

    /// <summary>
    /// Delete rule for the given rule id.
    /// </summary>
    void DeleteRule(Guid ruleId);

    /// <summary>
    /// Delete all rules.
    /// </summary>
    Task DeleteRules();
}
