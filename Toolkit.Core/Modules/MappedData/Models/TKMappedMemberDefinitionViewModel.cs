using System.Collections.Generic;

namespace QoDL.Toolkit.Core.Modules.MappedData.Models;

/// <summary></summary>
public class TKMappedMemberDefinitionViewModel
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
		public bool IsValid { get; set; }

		/// <summary></summary>
		public string Error { get; set; }

		/// <summary></summary>
		public List<TKMappedMemberDefinitionViewModel> Children { get; set; }

		/// <summary></summary>
		public List<TKMappedMemberReferenceDefinitionViewModel> MappedTo { get; set; }
	}
