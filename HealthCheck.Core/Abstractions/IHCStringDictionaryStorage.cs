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
}
