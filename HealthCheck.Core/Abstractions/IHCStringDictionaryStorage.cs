using HealthCheck.Core.Util;
using System.Collections.Generic;

namespace HealthCheck.Core.Abstractions
{
    /// <summary>
    /// Provides load/save capabilities for string dictionaries.
    /// </summary>
    public interface IHCStringDictionaryStorage
    {
        /// <summary>
        /// Load values.
        /// </summary>
        Dictionary<string, string> GetValues();

        /// <summary>
        /// Save the given values.
        /// </summary>
        void SaveValues(Dictionary<string, string> values);
    }

    /// <summary>
    /// Extensions for sett
    /// </summary>
#pragma warning disable S101 // Types should be named in PascalCase
    public static class IHCStringDictionaryStorageExtensions
#pragma warning restore S101 // Types should be named in PascalCase
    {
        /// <summary>
        /// Get stored values as the given model.
        /// </summary>
        public static TModel GetModel<TModel>(this IHCStringDictionaryStorage storage)
            where TModel : class, new()
        {
            // todo: need to cache this somehow
            var values = storage.GetValues();
            return HCValueConversionUtils.ConvertInputModel<TModel>(values);
        }
    }
}
