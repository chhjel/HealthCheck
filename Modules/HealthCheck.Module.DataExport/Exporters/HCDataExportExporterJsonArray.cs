using HealthCheck.Module.DataExport.Abstractions;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Text;

namespace HealthCheck.Module.DataExport.Exporters
{
    /// <summary>
    /// Outputs a json array.
    /// </summary>
    public class HCDataExportExporterJsonArray : IHCDataExportExporter
    {
		/// <inheritdoc />
		public string DisplayName { get; set; } = "JSON Array";

		/// <inheritdoc />
		public string FileExtension { get; set; } = ".json";

		/// <summary>
		/// When true json output will be prettified.
		/// <para>Defaults to true.</para>
		/// </summary>
		public bool Prettify { get; set; } = true;

		private readonly List<Dictionary<string, object>> _builder = new();

		/// <inheritdoc />
		public void SetHeaders(List<string> headers) { }

		/// <inheritdoc />
		public void AppendItem(Dictionary<string, object> items, Dictionary<string, string> itemsStringified, List<string> order)
			=> _builder.Add(items);

		/// <inheritdoc />
		public byte[] GetContents() => Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(_builder, Prettify ? Formatting.Indented : Formatting.None));
	}
}
