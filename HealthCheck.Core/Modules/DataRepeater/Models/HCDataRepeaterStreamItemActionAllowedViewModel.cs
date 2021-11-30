namespace HealthCheck.Core.Modules.DataRepeater.Models
{
    /// <summary>
    /// Viewmodel for action allowed result per item.
    /// </summary>
    public class HCDataRepeaterStreamItemActionAllowedViewModel
    {
        /// <summary>
        /// Id of related action.
        /// </summary>
        public string ActionId { get; set; }

        /// <summary>
        /// True if the action is allowed to be run.
        /// </summary>
        public bool Allowed { get; set; }

        /// <summary>
        /// Reason why the action can't be executed if any.
        /// </summary>
        public string Reason { get; set; }
    }
}
