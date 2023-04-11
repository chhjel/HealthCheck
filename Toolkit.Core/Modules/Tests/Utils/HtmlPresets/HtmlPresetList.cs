using System.Collections.Generic;
using System.Text;

namespace QoDL.Toolkit.Core.Modules.Tests.Utils.HtmlPresets
{
    /// <summary>
    /// A list. Can be either ol or ul.
    /// </summary>
    public class HtmlPresetList : IHtmlPreset
    {
        /// <summary>
        /// Items in the list.
        /// </summary>
        protected List<string> Items { get; set; } = new List<string>();

        /// <summary>
        /// Style of the list.
        /// </summary>
        protected HtmlPresetListStyle Style { get; set; }

        /// <summary>
        /// Style of the list.
        /// </summary>
        public enum HtmlPresetListStyle
        {
            /// <summary>
            /// ul-tag.
            /// </summary>
            Unordered = 0,

            /// <summary>
            /// ol-tag.
            /// </summary>
            Ordered
        }

        /// <summary>
        /// Creates a list of either ul- or ol-tags.
        /// </summary>
        public HtmlPresetList(HtmlPresetListStyle style = HtmlPresetListStyle.Unordered)
        {
            Style = style;
        }

        /// <summary>
        /// Creates a list of either ul- or ol-tags.
        /// </summary>
        public HtmlPresetList(IEnumerable<string> items, HtmlPresetListStyle style = HtmlPresetListStyle.Unordered) : this(style)
        {
            AddItems(items);
        }

        /// <summary>
        /// Creates a list of either ul- or ol-tags.
        /// </summary>
        public HtmlPresetList(HtmlPresetListStyle style = HtmlPresetListStyle.Unordered, params string[] items) : this(style)
        {
            AddItems(items);
        }

        /// <summary>
        /// Add another item to the list.
        /// </summary>
        public HtmlPresetList AddItem(string item)
        {
            Items.Add(item);
            return this;
        }

        /// <summary>
        /// Add more items to the list.
        /// </summary>
        public HtmlPresetList AddItems(IEnumerable<string> items)
        {
            Items.AddRange(items);
            return this;
        }

        /// <summary>
        /// Add more items to the list.
        /// </summary>
        public HtmlPresetList AddItems(params string[] items)
        {
            Items.AddRange(items);
            return this;
        }

        /// <summary>
        /// Create html from the data in this object.
        /// </summary>
        public virtual string ToHtml()
        {
            var builder = new StringBuilder();

            var element = (Style == HtmlPresetListStyle.Unordered)
                ? "ul" : "ol";

            builder.AppendLine($"<{element}>");
            foreach (var item in Items)
            {
                builder.AppendLine($"<li>{item}</li>");
            }
            builder.AppendLine($"</{element}>");

            return builder.ToString();
        }
    }
}