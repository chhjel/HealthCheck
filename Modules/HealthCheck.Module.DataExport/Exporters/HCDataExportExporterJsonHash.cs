using HealthCheck.Module.DataExport.Abstractions;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HealthCheck.Module.DataExport.Exporters
{
    /// <summary>
    /// Outputs a json hash using the first column as keys.
    /// </summary>
    public class HCDataExportExporterJsonHash : IHCDataExportExporter
	{
		/// <inheritdoc />
		public string DisplayName { get; set; } = "JSON Hash";

		/// <inheritdoc />
		public string FileExtension { get; set; } = ".json";

		/// <summary>
		/// When true json output will be prettified.
		/// <para>Defaults to true.</para>
		/// </summary>
		public bool Prettify { get; set; } = true;

		private readonly Dictionary<string, object> _builder = new();

		/// <inheritdoc />
		public void SetHeaders(List<string> headers) { }

		/// <inheritdoc />
		public void AppendItem(Dictionary<string, object> items, Dictionary<string, string> itemsStringified, List<string> order)
		{
			var keyKey = order?.FirstOrDefault();
			if (keyKey != null && itemsStringified.TryGetValue(keyKey, out var key))
			{
				var values = items.Where(x => x.Key != keyKey).ToDictionary(x => x.Key, x => x.Value);
				_builder[key] = values;
			}
		}

		/// <inheritdoc />
		public byte[] GetContents() => Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(_builder, Prettify ? Formatting.Indented : Formatting.None));
	}
}
