using System;

namespace QoDL.Toolkit.Module.EndpointControl.Models
{
    /// <summary>
    /// Model sent when requesting enabling/disabling of a rule.
    /// </summary>
    public class SetRuleEnabledRequestModel
    {
        /// <summary>
        /// Target rule id.
        /// </summary>
        public Guid RuleId { get; set; }
        
        /// <summary>
        /// Target new state.
        /// </summary>
        public bool Enabled { get; set; }
    }
}
