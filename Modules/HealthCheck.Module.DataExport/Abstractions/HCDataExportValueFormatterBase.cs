using System;

namespace HealthCheck.Module.DataExport.Abstractions
{
    /// <summary>
    /// Formats values of supported types.
    /// </summary>
    public abstract class HCDataExportValueFormatterBase<TParameters> : IHCDataExportValueFormatter
        where TParameters : class
    {
        /// <inheritdoc />
        public abstract string Name { get; }

        /// <inheritdoc />
        public abstract string Description { get; }

        /// <inheritdoc />
        public abstract Type[] SupportedTypes { get; }

        /// <inheritdoc />
        public Type CustomParametersType => typeof(TParameters);

        /// <inheritdoc />
        public object FormatValue(string propertyName, Type propertyType, object value, object parameters)
            => FormatValueTyped(propertyName, propertyType, value, parameters as TParameters);

        /// <summary>
        /// Format a value.
        /// </summary>
        protected abstract object FormatValueTyped(string propertyName, Type propertyType, object value, TParameters parameters);
    }
}
