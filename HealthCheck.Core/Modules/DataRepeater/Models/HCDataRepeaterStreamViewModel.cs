using System.Collections.Generic;

namespace HealthCheck.Core.Modules.DataRepeater.Models
{
    /// <summary></summary>
    public class HCDataRepeaterStreamViewModel
    {
        /// <summary></summary>
        public string Id { get; set; }

        /// <summary></summary>
        public string Name { get; internal set; }

        /// <summary></summary>
        public string ItemIdName { get; internal set; }

        /// <summary></summary>
        public string GroupName { get; internal set; }

        /// <summary></summary>
        public List<HCDataRepeaterStreamActionViewModel> Actions { get; set; } = new();
    }
}
