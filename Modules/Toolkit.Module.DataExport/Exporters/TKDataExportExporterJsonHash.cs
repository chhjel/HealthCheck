using QoDL.Toolkit.Core.Extensions;
using QoDL.Toolkit.Module.DataExport.Abstractions;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QoDL.Toolkit.Module.DataExport.Exporters;

/// <summary>
/// Outputs a json hash using the first column as keys.
/// </summary>
public class TKDataExportExporterJsonHash : ITKDataExportExporter
	{
		/// <inheritdoc />
		public string DisplayName { get; set; } = "JSON Hash";

		/// <inheritdoc />
		public string Description { get; set; } = "Uses the first column as keys in a json hash, with the remaining columns as values.";

		/// <inheritdoc />
		public string FileExtension { get; set; } = ".json";

		/// <summary>
		/// When true json output will be prettified.
		/// <para>Defaults to true.</para>
		/// </summary>
		public bool Prettify { get; set; } = true;

		private readonly Dictionary<string, object> _builder = new();

		/// <inheritdoc />
		public void SetHeaders(Dictionary<string, string> headers, List<string> headerOrder) { }

		/// <inheritdoc />
		public void AppendItem(Dictionary<string, object> items, Dictionary<string, string> headers, List<string> headerOrder)
		{
			var headerToUseAsKey = headerOrder.FirstOrDefault();
			if (headerToUseAsKey != null)
			{
				var values = items
					.Where(x => x.Key != headerToUseAsKey)
					.ToDictionaryIgnoreDuplicates(x => headers[x.Key], x => x.Value);
				var key = items[headerToUseAsKey]?.ToString();
				if (string.IsNullOrEmpty(key)) key = "_";
				_builder[key] = values;
			}
		}

		/// <inheritdoc />
		public byte[] GetContents() => Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(_builder, Prettify ? Formatting.Indented : Formatting.None));
}
