using System.Collections.Generic;

namespace HealthCheck.Core.Modules.MappedData.Models
{
    /// <summary>
    /// Left/right pair of mapped definitions.
    /// </summary>
    public class HCMappedClassesDefinition
	{
		/// <summary>
		/// Group name if specified.
		/// </summary>
		public string GroupName => Left?.GroupName ?? Right?.GroupName;

		/// <summary>
		/// The left side definition of the mapping.
		/// </summary>
		public HCMappedClassDefinition Left { get; set; }

		/// <summary>
		/// The right side definition of the mapping.
		/// </summary>
		public HCMappedClassDefinition Right { get; set; }

		/// <summary>
		/// Left/right pairs of the members from both <see cref="Left"/> and <see cref="Right"/>.
		/// </summary>
		public List<HCMappedMemberDefinitionPair> MemberPairs { get; set; }
	}
}
