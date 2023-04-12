using System;

namespace QoDL.Toolkit.Module.DataExport.Abstractions;

/// <summary>
/// Formats values of supported types.
/// </summary>
public interface ITKDataExportValueFormatter
{
    /// <summary>
    /// Name of the formatter to show in frontend.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Optional description of the formatter to show in frontend.
    /// </summary>
    string Description { get; }

    /// <summary>
    /// Property types this formatter is supported for, including derived types.
    /// </summary>
    Type[] SupportedTypes { get; }

    /// <summary>
    /// Property types this formatter is not supported for.
    /// </summary>
    Type[] NotSupportedTypes { get; }

    /// <summary>
    /// Type of custom parameters object if any.
    /// </summary>
    Type CustomParametersType { get; }

    /// <summary>
    /// Format a value.
    /// </summary>
    object FormatValue(string propertyName, Type propertyType, object value, object parameters);
}
