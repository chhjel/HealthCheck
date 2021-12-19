using HealthCheck.Core.Attributes;
using HealthCheck.Module.DataExport.Abstractions;
using HealthCheck.Module.DataExport.Services;
using System;
using System.Collections;
using System.Collections.Generic;
using static HealthCheck.Module.DataExport.Formatters.HCDataExportEnumerableValueFormatter;

namespace HealthCheck.Module.DataExport.Formatters
{
    /// <summary>
    /// Formats <see cref="IEnumerable"/>s.
    /// </summary>
    public class HCDataExportEnumerableValueFormatter : HCDataExportValueFormatterBase<Parameters>
    {
        /// <inheritdoc />
        public override string Name => "List formatter";

        /// <inheritdoc />
        public override string Description => "Formats lists of data.";

        /// <inheritdoc />
        public override Type[] SupportedTypes => new Type[] { typeof(IEnumerable) };

        /// <inheritdoc />
        public override Type[] NotSupportedTypes => new Type[] { typeof(string) };

        /// <inheritdoc />
        protected override object FormatValueTyped(string propertyName, Type propertyType, object value, Parameters parameters)
        {
            if (value is IEnumerable enumerable)
            {
                List<string> values = new();
                foreach (var item in enumerable)
                {
                    values.Add(HCDataExportService.SerializeOrStringifyValue(item));
                }
                return string.Join(parameters.Delimiter ?? string.Empty, values);
            }
            
            return value;
        }

        /// <summary>
        /// Parameters model.
        /// </summary>
        public class Parameters
        {
            /// <summary></summary>
            [HCCustomProperty(UIHints = Core.Models.HCUIHint.NotNull)]
            public string Delimiter { get; set; } = string.Empty;
        }
    }
}
