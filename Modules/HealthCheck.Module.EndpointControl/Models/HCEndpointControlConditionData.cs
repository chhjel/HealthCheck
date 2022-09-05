using System.Collections.Generic;

namespace HealthCheck.Module.EndpointControl.Models
{
    /// <summary></summary>
    public class HCEndpointControlConditionData
    {
        /// <summary></summary>
        public string ConditionId { get; set; }

        /// <summary></summary>
        public Dictionary<string, string> Parameters { get; set; }
    }
}
