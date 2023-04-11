using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Web;

namespace QoDL.Toolkit.Core.Modules.Tests.Utils.HtmlPresets
{
    /// <summary>
    /// A key-value list. Either a simple list or a 2-column table.
    /// </summary>
    public class HtmlPresetKeyValueList : IHtmlPreset
    {
        /// <summary>
        /// Items in the list.
        /// </summary>
        protected List<KeyValuePair<string, string>> Items { get; set; } = new List<KeyValuePair<string, string>>();

        /// <summary>
        /// Style in the list.
        /// </summary>
        protected HtmlPresetKeyValueListStyle Style { get; set; } = HtmlPresetKeyValueListStyle.Simple;

        /// <summary>
        /// Header in the first column.
        /// </summary>
        protected string KeyColumnHeader { get; set; }

        /// <summary>
        /// Header in the value column.
        /// </summary>
        protected string ValueColumnHeader { get; set; }

        /// <summary>
        /// Encode values.
        /// </summary>
        protected bool EncodeData { get; set; }

        /// <summary>
        /// Style of the list.
        /// </summary>
        protected enum HtmlPresetKeyValueListStyle
        {
            /// <summary>
            /// Each item on a new line, separated by br-tags.
            /// </summary>
            Simple = 0,

            /// <summary>
            /// A two-column table.
            /// </summary>
            Table
        }

        /// <summary>
        /// A key-value list. Either a simple list or a 2-column table.
        /// </summary>
        /// <param name="encodeData">True to html-encode values.</param>
        public HtmlPresetKeyValueList(bool encodeData = true)
        {
            EncodeData = encodeData;
        }

        /// <summary>
        /// A key-value list. Either a simple list or a 2-column table.
        /// </summary>
        /// <param name="key">The first key in the list.</param>
        /// <param name="value">The first value in the list.</param>
        /// <param name="encodeData">True to html-encode values.</param>
        /// <param name="excludeNullValues">If true null-values will be excluded.</param>
        public HtmlPresetKeyValueList(string key, string value, bool encodeData = true, bool excludeNullValues = true) : this(encodeData)
            => AddItem(key, value, excludeNullValues);

        /// <summary>
        /// A key-value list. Either a simple list or a 2-column table.
        /// </summary>
        /// <param name="values">List entries.</param>
        /// <param name="encodeData">True to html-encode values.</param>
        /// <param name="excludeNullValues">If true null-values will be excluded.</param>
        public HtmlPresetKeyValueList(Dictionary<string, string> values, bool encodeData = true, bool excludeNullValues = true) : this(encodeData)
        {
            values.ToList().ForEach((x) =>
            {
                if (x.Value != null || !excludeNullValues)
                {
                    Items.Add(new KeyValuePair<string, string>(x.Key, x.Value));
                }
            });
        }

        /// <summary>
        /// A key-value list. Either a simple list or a 2-column table.
        /// </summary>
        /// <param name="values">List entries.</param>
        /// <param name="encodeData">True to html-encode values.</param>
        /// <param name="excludeNullValues">If true null-values will be excluded.</param>
        public HtmlPresetKeyValueList(NameValueCollection values, bool encodeData = true, bool excludeNullValues = true) : this(encodeData)
        {
            values.AllKeys.ToList().ForEach((x) =>
            {
                if (values[x] != null || !excludeNullValues)
                {
                    Items.Add(new KeyValuePair<string, string>(x, values[x]));
                }
            });
        }

        /// <summary>
        /// Add an item to the list.
        /// </summary>
        public HtmlPresetKeyValueList AddItem(string key, string value, bool onlyIfNotNull = true)
        {
            if (value == null && onlyIfNotNull)
                return this;
            
            Items.Add(new KeyValuePair<string, string>(key, value));
            return this;
        }

        /// <summary>
        /// Use a 2-column table instead of a simple list.
        /// </summary>
        public HtmlPresetKeyValueList UseTableStyle(string keyColumnHeader = null, string valueColumnHeader = null)
        {
            Style = HtmlPresetKeyValueListStyle.Table;
            KeyColumnHeader = keyColumnHeader;
            ValueColumnHeader = valueColumnHeader;
            return this;
        }

        /// <summary>
        /// Create html from the data in this object.
        /// </summary>
        public virtual string ToHtml()
        {
            var builder = new StringBuilder();

            // Items
            if (Style == HtmlPresetKeyValueListStyle.Table)
            {
                builder.AppendLine($"<table>");
                builder.AppendLine($" <tr>");
                builder.AppendLine($"  <th>{KeyColumnHeader}</th>");
                builder.AppendLine($"  <th>{ValueColumnHeader}</th>");
                builder.AppendLine($" </tr>");
            }
            foreach (var item in Items)
            {
                var key = (EncodeData) ? HttpUtility.HtmlEncode(item.Key) : item.Key;
                var value = (EncodeData) ? HttpUtility.HtmlEncode(item.Value) : item.Value;
                if (Style == HtmlPresetKeyValueListStyle.Table)
                {
                    builder.AppendLine($" <tr>");
                    builder.AppendLine($"  <td>{key}</td>");
                    builder.AppendLine($"  <td>{value}</td>");
                    builder.AppendLine($" </tr>");
                }
                else
                {
                    builder.AppendLine($"<b>{key}:</b> {value}<br />");
                }
            }
            if (Style == HtmlPresetKeyValueListStyle.Table)
            {
                builder.AppendLine($"</table>");
            }

            return builder.ToString();
        }
    }
}