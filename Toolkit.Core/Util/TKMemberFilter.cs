using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace QoDL.Toolkit.Core.Util;

/// <summary>
/// Used to filter members.
/// </summary>
public class TKMemberFilter
	{
		/// <summary>
		/// Filter properties.
		/// </summary>
		public TKPropertyFilter PropertyFilter { get; set; } = new();

		/// <summary>
		/// Filter types.
		/// </summary>
		public TKTypeFilter TypeFilter { get; set; } = new();

		/// <summary>
		/// If true, only properties that are defined explicitly on the given type is included.
		/// </summary>
		public bool IncludeOwnedMembersOnly { get; set; }

		/// <summary>
		/// Optional list of types to ignore types declared from.
		/// </summary>
		public IEnumerable<Type> IgnoreMembersDeclaredInTypes { get; set; }

		/// <summary>
		/// Checks if a given property passes the filter.
		/// </summary>
		public virtual bool AllowMember(MemberInfo member)
		{
			var classType = member.DeclaringType;
			Type type = null;
			PropertyInfo property = null;
			if (member.MemberType == MemberTypes.Property && member is PropertyInfo prop)
			{
				property = prop;
				type = prop.PropertyType;
			}
			else if (member.MemberType == MemberTypes.Field && member is FieldInfo field) type = field.FieldType;

			if (type != null && TypeFilter?.AllowType(type) == false) return false;
			else if (property != null && PropertyFilter?.AllowProperty(property) == false) return false;
			else if (IgnoreMembersDeclaredInTypes?.Contains(classType) == true)
			{
				return false;
			}
			return true;
		}
	}
