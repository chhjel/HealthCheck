using System.Collections.Generic;

namespace HealthCheck.Core.Modules.MappedData.Models
{
    /// <summary></summary>
    public class HCMappedMemberDefinitionViewModel
	{
		/// <summary></summary>
		public string Id { get; set; }

		/// <summary></summary>
		public string PropertyName { get; set; }

		/// <summary></summary>
		public string PropertyTypeName { get; set; }

		/// <summary></summary>
		public string FullPropertyTypeName { get; set; }

		/// <summary></summary>
		public string FullPropertyPath { get; set; }

		/// <summary></summary>
		public string DisplayName { get; set; }

		/// <summary></summary>
		public string Remarks { get; set; }

		/// <summary></summary>
		public List<HCMappedMemberDefinitionViewModel> Children { get; set; }

		/// <summary></summary>
		public List<HCMappedMemberReferenceDefinitionViewModel> MappedTo { get; set; }
	}
}
