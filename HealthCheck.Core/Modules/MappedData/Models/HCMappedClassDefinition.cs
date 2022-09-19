using HealthCheck.Core.Modules.MappedData.Attributes;
using System;
using System.Collections.Generic;

namespace HealthCheck.Core.Modules.MappedData.Models
{
    /// <summary>
    /// A definitions of a class being mapped to something.
    /// </summary>
    public class HCMappedClassDefinition
	{
		/// <summary>
		/// Internally used id of this definition.
		/// </summary>
		public string Id { get; set; }

		/// <summary>
		/// Name of the class type.
		/// </summary>
		public string TypeName { get; set; }

		/// <summary>
		/// Display name of this mapping.
		/// </summary>
		public string DisplayName { get; set; }

		/// <summary>
		/// Group name if specified.
		/// </summary>
		public string GroupName { get; set; }

		/// <summary>
		/// Type of the class this definition belongs to.
		/// </summary>
		public Type ClassType { get; set; }

		/// <summary>
		/// Attribute decorated on the definition class type.
		/// </summary>
		public HCMappedClassAttribute Attribute { get; set; }

		/// <summary>
		/// Member definitions from any properties.
		/// </summary>
		public List<HCMappedMemberDefinition> MemberDefinitions { get; set; }

		/// <summary>
		/// Member definitions from all properties, including children of children etc.
		/// </summary>
		public List<HCMappedMemberDefinition> AllMemberDefinitions { get; set; }

		/// <summary></summary>
		public override string ToString() => $"[{Id}]";
	}
}
