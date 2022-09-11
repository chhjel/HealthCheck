using HealthCheck.Core.Modules.MappedData.Attributes;
using System.Reflection;

namespace HealthCheck.Core.Modules.MappedData.Models
{
    /// <summary>
    /// Definition of a class member that is mapped to another.
    /// </summary>
    public class HCMappedMemberDefinition
	{
		/// <summary>
		/// Internal id of this definition.
		/// </summary>
		public string Id { get; set; }

		/// <summary>
		/// Name of this member.
		/// </summary>
		public string PropertyName { get; set; }

		/// <summary>
		/// Display name of this definition.
		/// </summary>
		public string DisplayName { get; set; }

		/// <summary>
		/// Other member name this one is mapped to.
		/// </summary>
		public string MappedTo { get; set; }

		/// <summary>
		/// The property info behind this member.
		/// </summary>
		public PropertyInfo Member { get; set; }

		/// <summary>
		/// Full type name of this members type.
		/// </summary>
		public string FullTypeName => Member == null ? null : $"{Member?.PropertyType?.Namespace}.{Member?.PropertyType?.Name}";

		/// <summary>
		/// Attribute decorated on this members type.
		/// </summary>
		public HCMappedPropertyAttribute Attribute { get; set; }

		internal bool IsReferenced { get; set; }
	}
}
