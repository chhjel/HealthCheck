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
        public string Description { get; set; }

        /// <summary></summary>
        public string StreamItemsName { get; set; }

        /// <summary></summary>
        public string ItemIdName { get; set; }

        /// <summary></summary>
        public string RetryActionName { get; set; }

        /// <summary></summary>
        public string RetryDescription { get; set; }

        /// <inheritdoc />
        public bool ManualAnalyzeEnabled { get; set; }

        /// <inheritdoc />
        public string AnalyzeActionName { get; set; }

        /// <summary></summary>
        public string GroupName { get; set; }

        /// <inheritdoc />
        public virtual List<string> InitiallySelectedTags { get; set; } = new();

        /// <inheritdoc />
        public virtual List<string> FilterableTags { get; set; } = new();

        /// <summary></summary>
        public List<HCDataRepeaterStreamActionViewModel> Actions { get; set; } = new();

        /// <summary></summary>
        public List<HCDataRepeaterStreamBatchActionViewModel> BatchActions { get; set; } = new();
    }
}
