using System.Collections.Generic;

namespace HealthCheck.Core.Modules.MappedData.Models
{
    /// <summary></summary>
    public class HCMappedClassesDefinitionViewModel
	{
		/// <summary></summary>
		public string GroupName { get; set; }

		/// <summary></summary>
		public HCMappedClassDefinitionViewModel Left { get; set; }

		/// <summary></summary>
		public HCMappedClassDefinitionViewModel Right { get; set; }

		/// <summary></summary>
		public List<HCMappedMemberDefinitionPairViewModel> MemberPairs { get; set; }
	}
}
