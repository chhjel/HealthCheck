using System.Web;

namespace QoDL.Toolkit.Core.Modules.Tests.Utils.HtmlPresets
{
    /// <summary>
    /// An a-tag with the given content.
    /// </summary>
    public class HtmlPresetLink : IHtmlPreset
    {
        /// <summary>
        /// Text in the link.
        /// </summary>
        protected string Text { get; set; }

        /// <summary>
        /// Url in the link.
        /// </summary>
        protected string Url { get; set; }

        /// <summary>
        /// An a-tag with the given content.
        /// </summary>
        public HtmlPresetLink(string url, string text = null)
        {
            Text = text ?? url;
            Url = url;
        }

        /// <summary>
        /// Create html from the data in this object.
        /// </summary>
        public string ToHtml()
        {
            if (string.IsNullOrWhiteSpace(Text)) return string.Empty;

            var text = HttpUtility.HtmlEncode(Text);
            return $"<a href=\"{Url}\">{text}</a>";
        }
    }
}
