using QoDL.Toolkit.Core.Modules.MappedData.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace QoDL.Toolkit.Core.Modules.MappedData.Utils
{
    /// <summary>
    /// Utilities related to the mapped data module.
    /// </summary>
    public static class TKMappedDataUtils
	{
		/// <summary>
		/// Optionally transform values used in example data. Use to e.g. handle complex objects.
		/// </summary>
		public static Func<object, string> ExampleDataValueTransformer { get; set; }

		private static readonly Dictionary<Type, TKMappedExampleValueViewModel> _exampleData = new();

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
				
				_exampleData[key] = new TKMappedExampleValueViewModel
				{
					StoredAt = now,
					ClassType = key,
					Instance = data,
					DataTypeName = key.Name
				};
            }
		}

		internal static List<TKMappedExampleValueViewModel> GetExampleData()
		{
			lock (_exampleData)
			{
				return _exampleData.Values.ToList();
			}
		}
	}
}
