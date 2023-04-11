namespace QoDL.Toolkit.WebUI.Models
{
    /// <summary>
    /// Action to perform on page.
    /// </summary>
    public class TKResultPageAction
    {
        /// <summary>
        /// Type of action to perform.
        /// </summary>
        public TKResultPageActionType Type { get; private set; }

        /// <summary>
        /// Where to redirect.
        /// </summary>
        public string RedirectTarget { get; private set; }

        private TKResultPageAction(TKResultPageActionType type) => Type = type;

        /// <summary>
        /// Create a result that does nothing.
        /// </summary>
        public static TKResultPageAction CreateNoAction() => new(TKResultPageActionType.None);

        /// <summary>
        /// Create a result that refreshes the page.
        /// </summary>
        public static TKResultPageAction CreateRefresh() => new(TKResultPageActionType.Refresh);

        /// <summary>
        /// Create a result that redirects somewhere.
        /// </summary>
        public static TKResultPageAction CreateRedirect(string url) => new(TKResultPageActionType.Redirect) { RedirectTarget = url };

        /// <summary></summary>
        public enum TKResultPageActionType
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
