using System.Collections.Generic;

namespace HealthCheck.Core.Modules.MappedData.Models
{
    /// <summary></summary>
    public class HCMappedExampleValueViewModel
	{
		/// <summary></summary>
		public string DataTypeName { get; set; }

		/// <summary></summary>
		public Dictionary<string, string> Values { get; set; }
	}
}
