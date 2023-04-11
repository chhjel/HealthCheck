using System;

namespace QoDL.Toolkit.Core.Modules.MappedData.Attributes
{
    /// <summary>
    /// Decorate a class with this for it to be discovered by TK as a type referenced from a mapping.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Class, AllowMultiple = false)]
	public class TKMappedReferencedTypeAttribute : Attribute
	{
		/// <summary>
		/// Optionally override id to reference this type from mappings by.
		/// </summary>
		public string ReferenceId { get; set; }

		/// <summary>
		/// Optionally override display name of this property.
		/// </summary>
		public string DisplayName { get; set; }

		/// <summary>
		/// Optionally override display name of this property used in the mapping display.
		/// </summary>
		public string NameInMapping { get; set; }

		/// <summary>
		/// Optional notes to display in the UI.
		/// </summary>
		public string Remarks { get; set; }
	}
}
