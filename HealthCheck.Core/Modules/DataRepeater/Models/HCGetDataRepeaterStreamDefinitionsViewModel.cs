using System.Collections.Generic;

namespace HealthCheck.Core.Modules.DataRepeater.Models
{
    /// <summary></summary>
    public class HCGetDataRepeaterStreamDefinitionsViewModel
    {
        /// <summary></summary>
        public List<HCDataRepeaterStreamViewModel> Streams { get; set; } = new();
    }
}
