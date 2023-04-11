using System.Collections.Generic;

namespace QoDL.Toolkit.Core.Modules.MappedData.Models
{
    /// <summary>
    /// Container for definitions.
    /// </summary>
    public class TKMappedDataDefinitions
	{
		/// <summary>
		/// Class defs.
		/// </summary>
		public List<TKMappedClassDefinition> ClassDefinitions { get; set; } = new();

		/// <summary>
		/// Referenced defs.
		/// </summary>
		public List<TKMappedReferencedTypeDefinition> ReferencedDefinitions = new();
	}
}
