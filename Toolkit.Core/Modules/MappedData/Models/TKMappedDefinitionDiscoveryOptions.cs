using System.Reflection;

namespace QoDL.Toolkit.Core.Modules.MappedData.Models
{
    /// <summary>
    /// Options passed to <see cref="Utils.TKMappedDataDefinitionBuilder"/>.
    /// </summary>
    public class TKMappedDefinitionDiscoveryOptions
	{
		/// <summary>
		/// Checks if display name of a property is allowed to be attempted resolved by checking attributes for certain properties.
		/// </summary>
		public delegate bool AllowAttributeDisplayNameResolveDelegate(PropertyInfo member);

		/// <summary>
		/// Checks if display name of a property is allowed to be attempted resolved by checking attributes for certain properties.
		/// <para>Defaults to true if null.</para>
		/// </summary>
		public AllowAttributeDisplayNameResolveDelegate AllowAttributeDisplayNameResolve { get; set; }

		/// <summary>
		/// Override display name resolve logic.
		/// </summary>
		public delegate string ResolveMemberDisplayNameDelegate(PropertyInfo member);

		/// <summary>
		/// Override display name resolve logic.
		/// </summary>
		public ResolveMemberDisplayNameDelegate MemberDisplayNameOverride { get; set; }
	}
}
