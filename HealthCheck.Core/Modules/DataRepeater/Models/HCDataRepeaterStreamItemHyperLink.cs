namespace HealthCheck.Core.Modules.DataRepeater.Models
{
    /// <summary>
    /// An url and text.
    /// </summary>
    public class HCDataRepeaterStreamItemHyperLink
    {
        /// <summary>
        /// Link text.
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// Url.
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// Create a new link.
        /// </summary>
        public HCDataRepeaterStreamItemHyperLink(string text, string url)
        {
            Text = text;
            Url = url;
        }

        /// <summary>
        /// Create a new link.
        /// </summary>
        public HCDataRepeaterStreamItemHyperLink() { }
    }
}
