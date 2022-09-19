using System.Collections.Generic;

namespace HealthCheck.Core.Modules.MappedData.Models
{
    /// <summary>
    /// Container for definitions.
    /// </summary>
    public class HCMappedDataDefinitions
	{
		/// <summary>
		/// Class defs.
		/// </summary>
		public List<HCMappedClassDefinition> ClassDefinitions { get; set; } = new();

		/// <summary>
		/// Referenced defs.
		/// </summary>
		public List<HCMappedReferencedTypeDefinition> ReferencedDefinitions = new();
	}
}
