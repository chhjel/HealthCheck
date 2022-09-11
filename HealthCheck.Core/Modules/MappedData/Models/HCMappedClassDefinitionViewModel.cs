using System.Collections.Generic;

namespace HealthCheck.Core.Modules.MappedData.Models
{
    /// <summary></summary>
    public class HCMappedClassDefinitionViewModel
	{
		/// <summary></summary>
		public string Id { get; set; }

		/// <summary></summary>
		public string TypeName { get; set; }

		/// <summary></summary>
		public string DisplayName { get; set; }

		/// <summary></summary>
		public string MapsToDefinitionId { get; set; }

		/// <summary></summary>
		public string ClassTypeName { get; set; }

		/// <summary></summary>
		public string DataSourceName { get; set; }

		/// <summary></summary>
		public string GroupName { get; set; }

		/// <summary></summary>
		public string Remarks { get; set; }

		/// <summary></summary>
		public List<HCMappedMemberDefinitionViewModel> MemberDefinitions { get; set; }
	}
}
