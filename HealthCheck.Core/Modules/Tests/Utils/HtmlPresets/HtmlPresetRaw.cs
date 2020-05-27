using System;

namespace HealthCheck.Core.Modules.Tests.Utils.HtmlPresets
{
    /// <summary>
    /// Raw html.
    /// </summary>
    public class HtmlPresetRaw : IHtmlPreset
    {
        /// <summary>
        /// Html to output.
        /// </summary>
        protected string Html { get; set; }

        /// <summary>
        /// Raw html.
        /// </summary>
        public HtmlPresetRaw(string html)
        {
            Html = html;
        }

        /// <summary>
        /// Create html from the data in this object.
        /// </summary>
        public string ToHtml()
            => String.IsNullOrWhiteSpace(Html) ? String.Empty : Html;
    }
}