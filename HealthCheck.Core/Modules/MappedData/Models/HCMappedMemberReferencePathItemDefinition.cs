using System;
using System.Reflection;

namespace HealthCheck.Core.Modules.MappedData.Models
{
    /// <summary></summary>
    public class HCMappedMemberReferencePathItemDefinition
	{
		/// <summary></summary>
		public bool Success { get; set; }
		/// <summary></summary>
		public string Error { get; set; }
		/// <summary></summary>
		public bool IsHardCoded => HardCodedValue != null;
		/// <summary></summary>
		public string HardCodedValue { get; set; }
		/// <summary></summary>
		public string PropertyName { get; set; }
		/// <summary></summary>
		public string DisplayName { get; set; }
		/// <summary></summary>
		public Type DeclaringType { get; set; }
		/// <summary></summary>
		public Type PropertyType { get; set; }
		/// <summary></summary>
		public PropertyInfo PropertyInfo { get; set; }
    }
}
