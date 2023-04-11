using System.Collections.Generic;
using System.Reflection;

namespace QoDL.Toolkit.Core.Modules.MappedData.Models
{
    /// <summary>
    /// Definition of a class member that is mapped to another.
    /// </summary>
    public class TKMappedMemberDefinition
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
		/// Full dotted path of the property.
		/// </summary>
		public string FullPropertyPath { get; set; }

		/// <summary>
		/// Display name of this definition.
		/// </summary>
		public string DisplayName { get; set; }

		/// <summary>
		/// The property info behind this member.
		/// </summary>
		public PropertyInfo Member { get; set; }

		/// <summary>
		/// Any comments for this member.
		/// </summary>
		public string Remarks { get; set; }

		/// <summary>
		/// True if the property was found.
		/// </summary>
		public bool IsValid { get; set; }

		/// <summary>
		/// Any error if not valid.
		/// </summary>
		public string Error { get; set; }

		/// <summary>
		/// Parent if any.
		/// </summary>
		public TKMappedMemberDefinition Parent { get; set; }

		/// <summary>
		/// Child properties.
		/// </summary>
		public List<TKMappedMemberDefinition> Children { get; set; }

		/// <summary>
		/// What this member is mapped to.
		/// </summary>
		public List<TKMappedMemberReferenceDefinition> MappedTo { get; set; }

		/// <summary></summary>
		public override string ToString() => $"{Id}";
	}
}
