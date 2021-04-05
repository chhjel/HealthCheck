using HealthCheck.Core.Util.Models;
using System.Collections.Generic;

namespace HealthCheck.Module.EndpointControl.Models
{
    /// <summary></summary>
    public class EndpointControlCustomResultDefinitionViewModel
    {
        /// <summary></summary>
        public string Id { get; set; }

        /// <summary></summary>
        public string Name { get; set; }

        /// <summary></summary>
        public string Description { get; set; }

        /// <summary></summary>
        public List<HCBackendInputConfig> CustomProperties { get; set; }
    }
}
