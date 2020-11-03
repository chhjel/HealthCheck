using HealthCheck.Core.Abstractions.Modules;
using HealthCheck.Module.EndpointControl.Models;
using System;
using System.Collections.Generic;

namespace HealthCheck.Module.EndpointControl.Module
{
    /// <summary>
    /// Module for controlling specified endpoints.
    /// </summary>
    public class HCEndpointControlModule : HealthCheckModuleBase<HCEndpointControlModule.AccessOption>
    {
        private HCEndpointControlModuleOptions Options { get; }

        /// <summary>
        /// Module for controlling specified endpoints.
        /// </summary>
        public HCEndpointControlModule(HCEndpointControlModuleOptions options)
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
        public override object GetFrontendOptionsObject(HealthCheckModuleContext context) => null;

        /// <summary>
        /// Get config for this module.
        /// </summary>
        public override IHealthCheckModuleConfig GetModuleConfig(HealthCheckModuleContext context) => new HCEndpointControlModuleConfig();
        
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
            EditEndpointDefinitions = 1 << 1
        }

        #region Invokable
        /// <summary>
        /// Enable/disable rule with the given id.
        /// </summary>
        [HealthCheckModuleMethod]
        public object SetRuleEnabled(HealthCheckModuleContext context, SetRuleEnabledRequestModel model)
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
        [HealthCheckModuleMethod]
        public object DeleteRule(HealthCheckModuleContext context, Guid id)
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
        [HealthCheckModuleMethod]
        public EndpointControlRule CreateOrUpdateRule(HealthCheckModuleContext context, EndpointControlRule rule)
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
        /// Get all the stored rules.
        /// </summary>
        [HealthCheckModuleMethod]
        public EndpointControlDataViewModel GetData()
            => new EndpointControlDataViewModel
            {
                Rules = Options.RuleStorage.GetRules(),
                EndpointDefinitions = Options.DefinitionStorage.GetDefinitions()
            };

        /// <summary>
        /// Delete a definition with the given id.
        /// </summary>
        [HealthCheckModuleMethod]
        public object DeleteDefinition(HealthCheckModuleContext context, string endpointId)
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
        [HealthCheckModuleMethod]
        public object DeleteAllDefinitions(HealthCheckModuleContext context)
        {
            Options.DefinitionStorage.ClearAllDefinitions();
            context?.AddAuditEvent($"Deleted all endpoint definitions");
            return new { Success = true };
        }

        /// <summary>
        /// Get latest requests.
        /// </summary>
        [HealthCheckModuleMethod]
        public IEnumerable<EndpointRequestDetails> GetLatestRequests()
            => Options.HistoryStorage.GetLatestRequests(maxCount: 100);
        #endregion
    }
}
