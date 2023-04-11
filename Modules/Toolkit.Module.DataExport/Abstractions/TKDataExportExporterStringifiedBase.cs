using QoDL.Toolkit.Module.DataExport.Abstractions.Streams;
using QoDL.Toolkit.Module.DataExport.Models;
using QoDL.Toolkit.Module.DataExport.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace QoDL.Toolkit.Module.DataExport.Abstractions
{
    /// <summary>
    /// Stringifies items and calls <see cref="AppendStringifiedItem"/>.
    /// </summary>
    public abstract class TKDataExportExporterStringifiedBase : ITKDataExportExporter
    {
        /// <inheritdoc />
        public abstract string DisplayName { get; set; }

        /// <inheritdoc />
        public abstract string Description { get; set; }

        /// <inheritdoc />
        public abstract string FileExtension { get; set; }

        /// <inheritdoc />
        public virtual void SetHeaders(Dictionary<string, string> headers, List<string> headerOrder) { }

        /// <inheritdoc />
        public void AppendItem(Dictionary<string, object> items, Dictionary<string, string> headers, List<string> headerOrder)
        {
            var stringified = new Dictionary<string, string>();
            foreach (var kvp in items)
            {
                var val = items[kvp.Key];
                stringified[kvp.Key] = TKDataExportService.SerializeOrStringifyValue(val);
            }
            AppendStringifiedItem(items, stringified, headers, headerOrder);
        }

        /// <summary>
        /// Appends the given data.
        /// </summary>
        public abstract void AppendStringifiedItem(Dictionary<string, object> items, Dictionary<string, string> stringifiedItems, Dictionary<string, string> headers, List<string> headerOrder);

        /// <inheritdoc />
        public abstract byte[] GetContents();
    }
}
