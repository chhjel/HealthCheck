using System;
using System.Linq;

namespace HealthCheck.Core.Modules.MappedData.Attributes
{
    /// <summary>
    /// Decorate a property with this for it to be discovered by HC.
    /// <para>Specifies that this property is mapped to another one by name.</para>
    /// <para>Required on at least one side of the mapping.</para>
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
	public class HCMappedPropertyAttribute : Attribute
	{
		// todo: allow mapping to multiple by comma or using array prop. E.g. category codes.
		// - Turn MappedTo into a function IsMappedTo and check both props.
		// - Viewmodel takes array

		/// <summary>
		/// Name of the property this property is mapped to.
		/// </summary>
		public string MappedToOne { get; set; }

		/// <summary>
		/// Name of the properties this property is mapped to.
		/// </summary>
		public string[] MappedToMany { get; set; }

		internal bool IsMappedToAnything => !string.IsNullOrWhiteSpace(MappedToOne) || MappedToMany?.Any() == true;

		/// <summary>
		/// Optionally override display name of this property.
		/// </summary>
		public string OverrideName { get; set; }

		/// <summary>
		/// Optional notes to display in the UI.
		/// </summary>
		public string Remarks { get; set; }

		/// <summary>
		/// Decorate a property with this for it to be discovered by HC.
		/// <para>Specifies that this property is mapped to another one by name.</para>
		/// <para>Required on at least one side of the mapping.</para>
		/// </summary>
		public HCMappedPropertyAttribute(string mappedToMemberName)
		{
			MappedToOne = mappedToMemberName;
		}

		/// <summary>
		/// Decorate a property with this for it to be discovered by HC.
		/// <para>Specifies that this property is mapped to another one by name.</para>
		/// <para>Required on at least one side of the mapping.</para>
		/// </summary>
		public HCMappedPropertyAttribute(params string[] mappedToManyMemberNames)
		{
			MappedToMany = mappedToManyMemberNames;
		}

		internal bool IsMappedTo(string name)
			=> !string.IsNullOrWhiteSpace(name) && (MappedToOne == name || MappedToMany?.Contains(name) == true);
	}
}
