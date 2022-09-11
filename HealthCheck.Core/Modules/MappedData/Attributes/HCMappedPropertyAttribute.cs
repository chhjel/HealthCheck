using System;

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
        /// <summary>
        /// Name of the property this property is mapped to.
        /// </summary>
        public string MappedTo { get; set; }

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
			MappedTo = mappedToMemberName;
		}
	}
}
