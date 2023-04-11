using System;
using System.Collections.Generic;

namespace QoDL.Toolkit.Core.Modules.MappedData.Models;

/// <summary></summary>
public class TKMappedMemberReferenceDefinition
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
		public List<TKMappedMemberReferencePathItemDefinition> Items { get; set; } = new List<TKMappedMemberReferencePathItemDefinition>();
	}
