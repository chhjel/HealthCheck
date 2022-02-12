using HealthCheck.Core.Util.Models;
using System.Collections.Generic;

namespace HealthCheck.Core.Modules.DataRepeater.Models
{
    /// <summary></summary>
    public class HCDataRepeaterStreamBatchActionViewModel
    {
        /// <summary></summary>
        public string Id { get; set; }

        /// <summary></summary>
        public string Name { get; set; }

        /// <summary></summary>
        public string Description { get; set; }

        /// <summary></summary>
        public string ExecuteButtonLabel { get; set; }

        /// <summary></summary>
        public List<HCBackendInputConfig> ParameterDefinitions { get; set; } = new();
    }
}
