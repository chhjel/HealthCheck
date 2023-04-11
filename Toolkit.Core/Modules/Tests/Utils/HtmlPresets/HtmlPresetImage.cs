using System;

namespace QoDL.Toolkit.Core.Modules.Tests.Utils.HtmlPresets
{
    /// <summary>
    /// An img-tag with the given url.
    /// </summary>
    public class HtmlPresetImage : IHtmlPreset
    {
        /// <summary>
        /// Image url.
        /// </summary>
        protected string Url { get; set; }

        /// <summary>
        /// An img-tag with the given url.
        /// </summary>
        public HtmlPresetImage(string url)
        {
            Url = url;
        }

        /// <summary>
        /// Create html from the data in this object.
        /// </summary>
        public string ToHtml()
        {
            if (String.IsNullOrWhiteSpace(Url))
            {
                return String.Empty;
            }

            return $"<img src=\"{Url}\" />";
        }
    }
}