using HealthCheck.Core.Modules.MappedData.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HealthCheck.Core.Modules.MappedData.Utils
{
    /// <summary>
    /// Utilities related to the mapped data module.
    /// </summary>
    public static class HCMappedDataUtils
	{
		/// <summary>
		/// Optionally preprocess values used in example data.
		/// </summary>
		public static Func<string, string> ExampleDataValueFilter { get; set; }

		private static readonly Dictionary<Type, HCMappedExampleValueViewModel> _exampleData = new();

        /// <summary>
        /// Stores example data in memory from the given input that will be shown in the UI.
        /// <para>Only the latest of each provided type is stored.</para>
        /// </summary>
        /// <param name="data">Data to store in memory.</param>
        /// <param name="onlyIfAtLeastThisLongSince">If provided, this duration must have passed since the previous one was stored for the new one to be stored.</param>
        public static void SetExampleFor<TData>(TData data, TimeSpan? onlyIfAtLeastThisLongSince = null)
		{
			lock (_exampleData)
            {
				var key = typeof(TData);
				var now = DateTimeOffset.Now;
				if (onlyIfAtLeastThisLongSince != null && _exampleData.TryGetValue(key, out var existing))
                {
					if ((now - existing.StoredAt) < onlyIfAtLeastThisLongSince) return;
                }
				
				_exampleData[key] = new HCMappedExampleValueViewModel
				{
					StoredAt = now,
					ClassType = key,
					Instance = data,
					DataTypeName = key.Name
				};
            }
		}

		internal static List<HCMappedExampleValueViewModel> GetExampleData()
		{
			lock (_exampleData)
			{
				return _exampleData.Values.ToList();
			}
		}
	}
}
