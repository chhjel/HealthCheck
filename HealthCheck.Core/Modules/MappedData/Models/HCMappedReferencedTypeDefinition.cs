﻿using HealthCheck.Core.Modules.MappedData.Attributes;
using System;

namespace HealthCheck.Core.Modules.MappedData.Models
{
    /// <summary>
    /// Definition of a class being referenced from mapping.
    /// </summary>
    public class HCMappedReferencedTypeDefinition
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
		/// Display name of this definition.
		/// </summary>
		public string DisplayName { get; set; }

		/// <summary>
		/// Type of the decorated class.
		/// </summary>
		public Type Type { get; set; }

		/// <summary>
		/// Attribute decorated on the class type.
		/// </summary>
		public HCMappedReferencedTypeAttribute Attribute { get; set; }
	}
}
