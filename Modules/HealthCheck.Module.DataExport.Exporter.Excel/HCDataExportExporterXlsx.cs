using ClosedXML.Excel;
using HealthCheck.Module.DataExport.Abstractions;
using System.Collections.Generic;
using System.IO;

namespace HealthCheck.Module.DataExport.Exporter.Excel
{
    /// <summary>
    /// Outputs XLSX files.
    /// </summary>
    public class HCDataExportExporterXlsx : IHCDataExportExporter
    {
        /// <inheritdoc />
        public string DisplayName => "Excel (XLSX)";

        /// <inheritdoc />
        public string FileExtension => ".xlsx";

        /// <summary>
        /// Adjust column sizes to content with some min/max values.
        /// <para>Defaults to true.</para>
        /// </summary>
        public bool AdjustToContents { get; set; } = true;

        private List<string> _headers;
        private XLWorkbook _workbook;
        private IXLWorksheet _worksheet;
        private int _currentRow;

        /// <inheritdoc />
        public void SetHeaders(List<string> headers)
        {
            _headers = headers;

            _workbook = new XLWorkbook();
            _worksheet = _workbook.Worksheets.Add("Data");

            _currentRow = 1;
            for (int i=0; i<_headers.Count;i++)
            {
                _worksheet.Cell(_currentRow, i+1).Value = _headers[i];
                _worksheet.Cell(_currentRow, i + 1).Style.Font.Bold = true;
                _worksheet.Cell(_currentRow, i + 1).Style.Font.Bold = true;
            }
        }

        /// <inheritdoc />
        public void AppendItem(Dictionary<string, object> items, Dictionary<string, string> itemsStringified, List<string> order)
        {
            _currentRow++;

            var colNumber = 0;
            foreach (var key in order)
            {
                colNumber++;
                _worksheet.Cell(_currentRow, colNumber).Value = itemsStringified[key] ?? string.Empty;
            }
        }

        /// <inheritdoc />
        public byte[] GetContents()
        {
            if (AdjustToContents)
            {
                _worksheet.Columns().AdjustToContents(10.0, 80.0);
            }

            using var stream = new MemoryStream();
            _workbook.SaveAs(stream);
            var content = stream.ToArray();

            _workbook.Dispose();
            _workbook = null;
            _worksheet = null;

            return content;
        }
    }
}
