using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QoDL.Toolkit.Module.DataExport.Abstractions
{
    /// <summary>
    /// Outputs delimited separated files.
    /// </summary>
    public abstract class TKDataExportExporterDelimitedSeparatedBase : TKDataExportExporterStringifiedBase
    {
        /// <summary>
        /// Delimiter to separate values with.
        /// </summary>
        public abstract string Delimiter { get; }

        /// <summary>
        /// Replaces newlines with spaces.
        /// </summary>
        public bool RemoveNewLines { get; set; } = true;

        private readonly StringBuilder _builder = new();

        /// <inheritdoc />
        public override void SetHeaders(Dictionary<string, string> headers, List<string> headerOrder)
        {
            _builder.AppendLine(CreateLine(headerOrder.Select(x => headers[x])));
        }

        /// <inheritdoc />
        public override void AppendStringifiedItem(Dictionary<string, object> items, Dictionary<string, string> stringifiedItems, Dictionary<string, string> headers, List<string> headerOrder)
        {
            var parts = headerOrder.Select(x => stringifiedItems[x] ?? string.Empty);
            var line = CreateLine(parts);
            _builder.AppendLine(line);
        }

        /// <inheritdoc />
        public override byte[] GetContents() => Encoding.UTF8.GetBytes(_builder.ToString());

        /// <summary>
        /// Create a CSV line from the given raw parts.
        /// </summary>
        protected virtual string CreateLine(IEnumerable<string> parts)
            => string.Join(Delimiter, parts.Select(x => PrepareValue(x)));

        /// <summary>
        /// Called on each raw column value to encode it for csv.
        /// <para>Replaces newlines with spaces if <see cref="RemoveNewLines"/> is true.</para>
        /// <para>Quotes value if value contains any <see cref="Delimiter"/></para>
        /// </summary>
        protected virtual string PrepareValue(string value)
        {
            if (value == null) return "";

            if (RemoveNewLines)
            {
                value = value
                    .Replace("\r\n", " ")
                    .Replace("\n\r", " ")
                    .Replace("\r", " ")
                    .Replace("\n", " ");
            }

            if (value.Contains(Delimiter))
            {
                var escaped = value.Replace("\"", "\"\"");
                value = $"\"{escaped}\"";
            }

            return value;
        }
    }
}
