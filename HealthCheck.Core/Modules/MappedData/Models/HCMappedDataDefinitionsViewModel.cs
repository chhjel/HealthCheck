using System.Collections.Generic;

namespace HealthCheck.Core.Modules.MappedData.Models
{
    /// <summary></summary>
    public class HCMappedDataDefinitionsViewModel
	{
		/// <summary></summary>
		public List<HCMappedClassDefinitionViewModel> ClassDefinitions { get; set; } = new();

		/// <summary></summary>
		public List<HCMappedReferencedTypeDefinitionViewModel> ReferencedDefinitions = new();
	}
}
