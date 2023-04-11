using System;
using System.Collections.Generic;

namespace QoDL.Toolkit.Core.Modules.Settings.Abstractions
{
    /// <summary>
    /// Provides load/save capabilities for string dictionaries with caching.
    /// </summary>
    public interface ITKSettingsService
    {
        /// <summary>
        /// Get stored values as the given model.
        /// </summary>
        TModel GetSettings<TModel>()
            where TModel : class, new();

        /// <summary>
        /// Get stored values.
        /// </summary>
        Dictionary<string, string> GetSettingValues(Type type);

        /// <summary>
        /// Save the given model.
        /// </summary>
        void SaveSettings<TModel>(TModel model)
            where TModel : class, new();

        /// <summary>
        /// Save the given model.
        /// </summary>
        void SaveSettings(Type type, object model);

        /// <summary>
        /// Save the given values.
        /// </summary>
        void SaveSettings(Type type, Dictionary<string, string> values);
    }
}
