using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace HealthCheck.Core.Util
{
    /// <summary>
    /// Used to filter properties.
    /// </summary>
    public class HCPropertyFilter
	{
		/// <summary>
		/// If true, excludes generated, special, readonly, indexer etc properties.
		/// <para>Default true.</para>
		/// </summary>
		public bool ExcludeSpecialEtcProperties { get; set; }

		/// <summary>
		/// Checks if a given property passes the filter.
		/// </summary>
		public virtual bool AllowProperty(PropertyInfo prop)
		{
			if (ExcludeSpecialEtcProperties
				&& (prop.IsSpecialName
				|| prop.GetMethod == null
				|| !prop.CanRead
				|| prop.GetCustomAttribute<CompilerGeneratedAttribute>() != null
				|| prop.GetIndexParameters()?.Any() == true))
			{
				return false;
			}
			return true;
		}
	}
}
