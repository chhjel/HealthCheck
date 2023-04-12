using QoDL.Toolkit.Core.Attributes;
using QoDL.Toolkit.Module.DataExport.Abstractions;
using System;
using static QoDL.Toolkit.Module.DataExport.Formatters.TKDataExportDateTimeValueFormatter;

namespace QoDL.Toolkit.Module.DataExport.Formatters;

/// <summary>
/// Formats <see cref="DateTime"/>s and <see cref="DateTimeOffset"/>s.
/// </summary>
public class TKDataExportDateTimeValueFormatter : TKDataExportValueFormatterBase<Parameters>
{
    /// <inheritdoc />
    public override string Name => "Date formatter";

    /// <inheritdoc />
    public override string Description => "Formats dates with a custom format string.";

    /// <inheritdoc />
    public override Type[] SupportedTypes => new Type[] { typeof(DateTime), typeof(DateTimeOffset) };

    /// <inheritdoc />
    protected override object FormatValueTyped(string propertyName, Type propertyType, object value, Parameters parameters)
    {
        if (value is DateTime dateTime) return dateTime.ToString(parameters.Format);
        else if (value is DateTimeOffset dateTimeOffset) return dateTimeOffset.ToString(parameters.Format);
        else return value;
    }

    /// <summary>
    /// Parameters model.
    /// </summary>
    public class Parameters
    {
        /// <summary></summary>
        [TKCustomProperty(UIHints = Core.Models.TKUIHint.NotNull)]
        public string Format { get; set; } = "dd/MM/yyyy HH:mm:ss";
    }
}
