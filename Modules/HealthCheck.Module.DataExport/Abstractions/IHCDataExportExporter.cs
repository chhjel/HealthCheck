using System.Collections.Generic;

namespace HealthCheck.Module.DataExport.Abstractions
{
    /// <summary>
    /// Type of output data to be exported.
    /// </summary>
    public interface IHCDataExportExporter
    {
        /// <summary>
        /// Name shown in the UI.
        /// </summary>
        string DisplayName { get; }

        /// <summary>
        /// Extension used for filename.
        /// </summary>
        string FileExtension { get; }

        /// <summary>
        /// Sets the headers for this data.
        /// </summary>
        void SetHeaders(List<string> headers);

        /// <summary>
        /// Append a new item to be exported.
        /// </summary>
        void AppendItem(Dictionary<string, object> items, Dictionary<string, string> itemsStringified, List<string> order);

        /// <summary>
        /// Get current contents.
        /// </summary>
        byte[] GetContents();
    }
}
