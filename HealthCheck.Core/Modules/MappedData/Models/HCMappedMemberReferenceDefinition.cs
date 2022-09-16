using System;
using System.Collections.Generic;

namespace HealthCheck.Core.Modules.MappedData.Models
{
    /// <summary></summary>
    public class HCMappedMemberReferenceDefinition
	{
		/// <summary></summary>
		public bool Success { get; set; }
		/// <summary></summary>
		public string Error { get; set; }
		/// <summary></summary>
		public string Path { get; set; }
		/// <summary></summary>
		public Type RootType { get; set; }
		/// <summary></summary>
		public string RootReferenceId { get; set; }
		/// <summary></summary>
		public List<HCMappedMemberReferencePathItemDefinition> Items { get; set; } = new List<HCMappedMemberReferencePathItemDefinition>();
	}
}
