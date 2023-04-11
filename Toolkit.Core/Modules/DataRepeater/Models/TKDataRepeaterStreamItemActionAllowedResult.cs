using QoDL.Toolkit.Core.Modules.DataRepeater.Abstractions;

namespace QoDL.Toolkit.Core.Modules.DataRepeater.Models
{
    /// <summary>
    /// Result from <see cref="ITKDataRepeaterStreamItemAction.ActionIsAllowedForAsync"/>
    /// </summary>
    public class TKDataRepeaterStreamItemActionAllowedResult
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
        public static TKDataRepeaterStreamItemActionAllowedResult CreateAllowed() => new() { Allowed = true};

        /// <summary></summary>
        public static TKDataRepeaterStreamItemActionAllowedResult CreateNotAllowed(string reason) => new() { Reason = reason };
    }
}
