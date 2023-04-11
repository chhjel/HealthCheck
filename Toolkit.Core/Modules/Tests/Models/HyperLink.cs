namespace QoDL.Toolkit.Core.Modules.Tests.Models
{
    /// <summary>
    /// An url and text.
    /// </summary>
    public class HyperLink
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
        public HyperLink(string text, string url)
        {
            Text = text;
            Url = url;
        }

        /// <summary>
        /// Create a new link.
        /// </summary>
        public HyperLink() {}
    }
}
