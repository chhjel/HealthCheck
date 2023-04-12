using Newtonsoft.Json;
using QoDL.Toolkit.Core.Extensions;
using QoDL.Toolkit.Module.DataExport.Abstractions;
using System.Collections.Generic;
using System.Text;

namespace QoDL.Toolkit.Module.DataExport.Exporters;

/// <summary>
/// Outputs a json array.
/// </summary>
public class TKDataExportExporterJsonArray : ITKDataExportExporter
{
    /// <inheritdoc />
    public string DisplayName { get; set; } = "JSON Array";

    /// <inheritdoc />
    public string Description { get; set; } = "Creates a json array of object with columns as values.";

    /// <inheritdoc />
    public string FileExtension { get; set; } = ".json";

    /// <summary>
    /// When true json output will be prettified.
    /// <para>Defaults to true.</para>
    /// </summary>
    public bool Prettify { get; set; } = true;

    private readonly List<Dictionary<string, object>> _builder = new();

    /// <inheritdoc />
    public void SetHeaders(Dictionary<string, string> headers, List<string> headerOrder) { }

    /// <inheritdoc />
    public void AppendItem(Dictionary<string, object> items, Dictionary<string, string> headers, List<string> headerOrder)
    {
        var itemsRenamed = headerOrder
            .ToDictionaryIgnoreDuplicates(x => headers[x], x => items[x]);

        _builder.Add(itemsRenamed);
    }

    /// <inheritdoc />
    public byte[] GetContents() => Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(_builder, Prettify ? Formatting.Indented : Formatting.None));
}
