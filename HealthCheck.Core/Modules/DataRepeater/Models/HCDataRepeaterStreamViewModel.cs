using System.Collections.Generic;

namespace HealthCheck.Core.Modules.DataRepeater.Models
{
    /// <summary></summary>
    public class HCDataRepeaterStreamViewModel
    {
        /// <summary></summary>
        public string Id { get; set; }

        /// <summary></summary>
        public string Name { get; set; }

        /// <summary></summary>
        public string ItemIdName { get; set; }

        /// <summary></summary>
        public string RetryActionName { get; set; }

        /// <summary></summary>
        public string RetryDescription { get; set; }

        /// <summary></summary>
        public string GroupName { get; set; }

        /// <summary></summary>
        public List<HCDataRepeaterStreamActionViewModel> Actions { get; set; } = new();
    }
}
