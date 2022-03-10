using System;
using System.Collections.Generic;
using System.Linq;

namespace HealthCheck.Core.Util
{
	/// <summary>
	/// Used to filter types.
	/// </summary>
	public class HCTypeFilter
	{
		/// <summary>
		/// Optional custom filter logic for members to include.
		/// </summary>
		public Func<Type, bool> IncludedMemberTypesFilter { get; set; }

		/// <summary>
		/// Optional list of member type namespace prefixes to ignore.
		/// </summary>
		public IEnumerable<string> IgnoredMemberTypeNamespacePrefixes { get; set; }

		/// <summary>
		/// Optional list of member types ignore, including descendants.
		/// </summary>
		public IEnumerable<Type> IgnoredMemberTypesIncludingDescendants { get; set; }

		/// <summary>
		/// Optional list of member types ignore.
		/// </summary>
		public IEnumerable<Type> IgnoredMemberTypes { get; set; }

		/// <summary>
		/// Optional list of generic type definitions to ignore, including descendants.
		/// </summary>
		public IEnumerable<Type> IgnoredMemberGenericTypeDefinitionsIncludingDescendants { get; set; }

		/// <summary>
		/// Optional list of generic type definitions to ignore.
		/// </summary>
		public IEnumerable<Type> IgnoredMemberGenericTypeDefinitions { get; set; }

		/// <summary>
		/// If true, all generic members that inherit from IEnumerable will be ignored.
		/// </summary>
		public bool IgnoreGenericEnumerableMemberTypes { get; set; }

		/// <summary>
		/// Checks if a given property passes the filter.
		/// </summary>
		public virtual bool AllowType(Type type)
		{
			if (IgnoreGenericEnumerableMemberTypes && type.IsGenericType && typeof(System.Collections.IEnumerable).IsAssignableFrom(type))
			{
				return false;
			}
			else if (IgnoredMemberTypes?.Contains(type) == true
				|| IgnoredMemberTypesIncludingDescendants?.Any(t => t.IsAssignableFrom(type)) == true)
			{
				return false;
			}

			var genericTypeDef = type.IsGenericType ? type.GetGenericTypeDefinition() : null;
			if (IgnoredMemberGenericTypeDefinitions?.Contains(genericTypeDef) == true
				|| IgnoredMemberGenericTypeDefinitionsIncludingDescendants?.Any(t => t.IsAssignableFrom(genericTypeDef)) == true)
			{
				return false;
			}
			else if (IgnoredMemberTypeNamespacePrefixes?.Any(p => type.Namespace?.ToLower()?.StartsWith(p?.ToLower() ?? string.Empty) == true) == true)
			{
				return false;
			}
			else
			{
				return IncludedMemberTypesFilter?.Invoke(type) != false;
			}
		}
	}
}
