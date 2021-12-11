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
        public string FileExtension => ".txt";

        /// <summary>
        /// Delimiter to separate values with.
        /// </summary>
        public string Delimiter { get; set; } = ";";

        private readonly StringBuilder _builder = new();

        /// <inheritdoc />
        public void SetHeaders(List<string> headers)
        {
            _builder.AppendLine(CreateLine(headers));
        }

        /// <inheritdoc />
        public void AppendItem(Dictionary<string, object> item, List<string> order)
        {
            var parts = order.Select(x => item[x]?.ToString());
            var line = CreateLine(parts);
            _builder.AppendLine(line);
        }

        /// <inheritdoc />
        public string GetContents() => _builder.ToString();

        private string CreateLine(IEnumerable<string> parts)
            => string.Join(Delimiter, parts.Select(x => PrepareValue(x)));

        private string PrepareValue(string value)
        {
            if (value == null) return "";
            if (value.Contains(Delimiter))
            {
                var escaped = value.Replace("\"", "\"\"");
                return $"\"{escaped}\"";
            }
            return value;
        }
    }
}
