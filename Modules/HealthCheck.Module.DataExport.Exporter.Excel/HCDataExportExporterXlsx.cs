using ClosedXML.Excel;
using HealthCheck.Module.DataExport.Abstractions;
using System.Collections.Generic;
using System.IO;

namespace HealthCheck.Module.DataExport.Exporter.Excel
{
    /// <summary>
    /// Outputs XLSX files.
    /// </summary>
    public class HCDataExportExporterXlsx : HCDataExportExporterStringifiedBase
    {
        /// <inheritdoc />
        public override string DisplayName { get; set; } = "Excel (XLSX)";

        /// <inheritdoc />
        public override string FileExtension { get; set; } = ".xlsx";

        /// <summary>
        /// Adjust column sizes to content with some min/max values.
        /// <para>Defaults to true.</para>
        /// </summary>
        public bool AdjustToContents { get; set; } = true;

        private XLWorkbook _workbook;
        private IXLWorksheet _worksheet;
        private int _currentRow;

        /// <inheritdoc />
        public override void SetHeaders(Dictionary<string, string> headers, List<string> headerOrder)
        {
            _workbook = new XLWorkbook();
            _worksheet = _workbook.Worksheets.Add("Data");

            _currentRow = 1;
            for (int i=0; i < headerOrder.Count; i++)
            {
                _worksheet.Cell(_currentRow, i + 1).Value = headers[headerOrder[i]];
                _worksheet.Cell(_currentRow, i + 1).Style.Font.Bold = true;
                _worksheet.Cell(_currentRow, i + 1).Style.Font.Bold = true;
            }
        }

        /// <inheritdoc />
        public override void AppendStringifiedItem(Dictionary<string, object> items, Dictionary<string, string> stringifiedItems, Dictionary<string, string> headers, List<string> headerOrder)
        {
            _currentRow++;

            var colNumber = 0;
            foreach (var header in headerOrder)
            {
                colNumber++;
                _worksheet.Cell(_currentRow, colNumber).Value = stringifiedItems[header] ?? string.Empty;
            }
        }

        /// <inheritdoc />
        public override byte[] GetContents()
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
