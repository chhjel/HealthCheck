using System;
using System.Text;

namespace HealthCheck.Core.Modules.Tests.Utils.HtmlPresets
{
    /// <summary>
    /// A title with size and color.
    /// </summary>
    public class HtmlPresetHeader : IHtmlPreset
    {
        /// <summary>
        /// Header text.
        /// </summary>
        protected string Text { get; set; }

        /// <summary>
        /// Header color.
        /// </summary>
        protected string Color { get; set; }

        /// <summary>
        /// Header size.
        /// </summary>
        protected HtmlPresetHeaderSize Size { get; set; }

        /// <summary>
        /// Header sizes.
        /// </summary>
        public enum HtmlPresetHeaderSize
        {
            /// <summary>
            /// H5
            /// </summary>
            Tiny = 0,

            /// <summary>
            /// H4
            /// </summary>
            Small,

            /// <summary>
            /// H3
            /// </summary>
            Normal,

            /// <summary>
            /// H2
            /// </summary>
            Large,

            /// <summary>
            /// H1
            /// </summary>
            Huge
        }

        /// <summary>
        /// A title with size and color.
        /// </summary>
        /// <param name="text">The header text</param>
        /// <param name="size">Size of the header</param>
        /// <param name="color">Optional color in css format</param>
        public HtmlPresetHeader(string text, HtmlPresetHeaderSize size = HtmlPresetHeaderSize.Normal, string color = null)
        {
            Text = text;
            Size = size;
            Color = color;
        }

        /// <summary>
        /// Create html from the data in this object.
        /// </summary>
        public string ToHtml()
        {
            if (String.IsNullOrWhiteSpace(Text))
            {
                return string.Empty;
            }

            var builder = new StringBuilder();

            var hSize = GetHSize();
            var styleAttribute = "";
            if (!String.IsNullOrWhiteSpace(Color))
            {
                styleAttribute = $" style=\"color: {Color};\"";
            }

            builder.Append($"<h{hSize}{styleAttribute}>{Text}</h{hSize}>");

            return builder.ToString();
        }

        private int GetHSize()
        {
            switch (Size)
            {
                case HtmlPresetHeaderSize.Huge: return 1;
                case HtmlPresetHeaderSize.Large: return 2;
                case HtmlPresetHeaderSize.Normal: return 3;
                case HtmlPresetHeaderSize.Small: return 4;
                case HtmlPresetHeaderSize.Tiny: return 5;
                default: return 3;
            }
        }
    }
}