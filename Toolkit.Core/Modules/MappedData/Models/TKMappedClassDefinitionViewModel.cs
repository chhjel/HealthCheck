using System.Collections.Generic;

namespace QoDL.Toolkit.Core.Modules.MappedData.Models;

/// <summary></summary>
public class TKMappedClassDefinitionViewModel
	{
		/// <summary></summary>
		public string Id { get; set; }

		/// <summary></summary>
		public string TypeName { get; set; }

		/// <summary></summary>
		public string DisplayName { get; set; }

		/// <summary></summary>
		public string ClassTypeName { get; set; }

		/// <summary></summary>
		public string GroupName { get; set; }

		/// <summary></summary>
		public string Remarks { get; set; }

		/// <summary></summary>
		public List<TKMappedMemberDefinitionViewModel> MemberDefinitions { get; set; }
	}
