using HealthCheck.Core.Modules.DataRepeater.Abstractions;

namespace HealthCheck.Core.Modules.DataRepeater.Models
{
    /// <summary>
    /// Result from <see cref="IHCDataRepeaterStreamItemAction.ActionIsAllowedForAsync"/>
    /// </summary>
    public class HCDataRepeaterStreamItemActionAllowedResult
    {
        /// <summary>
        /// True if the action is allowed to be run.
        /// </summary>
        public bool Allowed { get; set; }

        /// <summary>
        /// Reason why the action can't be executed if any.
        /// </summary>
        public string Reason { get; set; }

        /// <summary></summary>
        public static HCDataRepeaterStreamItemActionAllowedResult CreateAllowed() => new() { Allowed = true};

        /// <summary></summary>
        public static HCDataRepeaterStreamItemActionAllowedResult CreateNotAllowed(string reason) => new() { Reason = reason };
    }
}
