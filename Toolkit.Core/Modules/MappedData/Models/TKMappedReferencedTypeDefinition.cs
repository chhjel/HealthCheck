using QoDL.Toolkit.Core.Modules.MappedData.Attributes;
using System;

namespace QoDL.Toolkit.Core.Modules.MappedData.Models;

/// <summary>
/// Definition of a class being referenced from mapping.
/// </summary>
public class TKMappedReferencedTypeDefinition
	{
		/// <summary>
		/// Internal id.
		/// </summary>
		public string Id { get; set; }

		/// <summary>
		/// How to reference this class from mappings. Defaults to type name without namespace.
		/// </summary>
		public string ReferenceId { get; set; }

		/// <summary>
		/// Name of the class type.
		/// </summary>
		public string TypeName { get; set; }

		/// <summary>
		/// Display name of this definition.
		/// </summary>
		public string DisplayName { get; set; }

		/// <summary>
		/// Optionally override display name of this property used in the mapping display.
		/// </summary>
		public string NameInMapping { get; set; }

		/// <summary>
		/// Type of the decorated class.
		/// </summary>
		public Type Type { get; set; }

		/// <summary>
		/// Attribute decorated on the class type.
		/// </summary>
		public TKMappedReferencedTypeAttribute Attribute { get; set; }
}
