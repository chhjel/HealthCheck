using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace QoDL.Toolkit.Core.Util
{
	/// <summary>
	/// Used to filter members recursively.
	/// </summary>
	public class TKMemberFilterRecursive : TKMemberFilter
	{
		/// <summary>
		/// Optional list of member path prefixes to ignore.
		/// <para>E.g. <c>Name</c> or <c>Contact.FullName</c>.</para>
		/// </summary>
		public IEnumerable<string> IgnoredMemberPathPrefixes { get; set; }

        /// <summary>
        /// Checks if a given property passes the filter.
        /// </summary>
        public bool AllowMember(MemberInfo member, string recursiveMemberPath)
		{
			if (!base.AllowMember(member))
			{
				return false;
			}
			else
			{
				return IgnoredMemberPathPrefixes?.Any(p => recursiveMemberPath?.ToLower()?.StartsWith(p?.ToLower() ?? string.Empty) == true) != false;
			}
		}
	}
}
