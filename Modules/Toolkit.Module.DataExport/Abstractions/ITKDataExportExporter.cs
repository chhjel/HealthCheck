using System.Collections.Generic;

namespace QoDL.Toolkit.Module.DataExport.Abstractions
{
    /// <summary>
    /// Type of output data to be exported.
    /// </summary>
    public interface ITKDataExportExporter
    {
        /// <summary>
        /// Name shown in the UI.
        /// </summary>
        string DisplayName { get; }

        /// <summary>
        /// Description shown in the UI.
        /// </summary>
        string Description { get; }

        /// <summary>
        /// Extension used for filename.
        /// </summary>
        string FileExtension { get; }

        /// <summary>
        /// Sets the headers for this data.
        /// </summary>
        void SetHeaders(Dictionary<string, string> headers, List<string> headerOrder);

        /// <summary>
        /// Append a new item to be exported.
        /// </summary>
        void AppendItem(Dictionary<string, object> items, Dictionary<string, string> headers, List<string> headerOrder);

        /// <summary>
        /// Get current contents.
        /// </summary>
        byte[] GetContents();
    }
}
