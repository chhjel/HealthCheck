namespace HealthCheck.WebUI.Models
{
    /// <summary>
    /// Action to perform on page.
    /// </summary>
    public class HCResultPageAction
    {
        /// <summary>
        /// Type of action to perform.
        /// </summary>
        public HCResultPageActionType Type { get; private set; }

        /// <summary>
        /// Where to redirect.
        /// </summary>
        public string RedirectTarget { get; private set; }

        private HCResultPageAction(HCResultPageActionType type) => Type = type;

        /// <summary>
        /// Create a result that does nothing.
        /// </summary>
        public static HCResultPageAction CreateNoAction() => new(HCResultPageActionType.None);

        /// <summary>
        /// Create a result that refreshes the page.
        /// </summary>
        public static HCResultPageAction CreateRefresh() => new(HCResultPageActionType.Refresh);

        /// <summary>
        /// Create a result that redirects somewhere.
        /// </summary>
        public static HCResultPageAction CreateRedirect(string url) => new(HCResultPageActionType.Redirect) { RedirectTarget = url };

        /// <summary></summary>
        public enum HCResultPageActionType
        {
            /// <summary></summary>
            None = 0,
            /// <summary></summary>
            Refresh,
            /// <summary></summary>
            Redirect
        }
    }
}
