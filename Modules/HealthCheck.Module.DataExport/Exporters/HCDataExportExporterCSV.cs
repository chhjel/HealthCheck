using HealthCheck.Module.DataExport.Abstractions;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HealthCheck.Module.DataExport.Exporters
{
    /// <summary>
    /// Outputs CSV.
    /// </summary>
    public class HCDataExportExporterCSV : IHCDataExportExporter
    {
        /// <inheritdoc />
        public string DisplayName => "CSV";

        /// <inheritdoc />
        public string FileExtension => ".csv";

        /// <summary>
        /// Delimiter to separate values with.
        /// </summary>
        public string Delimiter { get; set; } = ";";

        /// <summary>
        /// Replaces newlines with spaces.
        /// </summary>
        public bool RemoveNewLines { get; set; } = true;

        private readonly StringBuilder _builder = new();

        /// <inheritdoc />
        public void SetHeaders(List<string> headers)
        {
            _builder.AppendLine(CreateLine(headers));
        }

        /// <inheritdoc />
        public void AppendItem(Dictionary<string, object> items, Dictionary<string, string> itemsStringified, List<string> order)
        {
            var parts = order.Select(x => itemsStringified[x] ?? string.Empty);
            var line = CreateLine(parts);
            _builder.AppendLine(line);
        }

        /// <inheritdoc />
        public byte[] GetContents() => Encoding.UTF8.GetBytes(_builder.ToString());

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
