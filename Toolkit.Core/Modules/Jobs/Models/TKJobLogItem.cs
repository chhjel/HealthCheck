using System;

namespace QoDL.Toolkit.Core.Modules.Jobs.Models
{
    /// <summary>
    /// 
    /// </summary>
    public class TKJobLogItem
    {
        /// <summary></summary>
        public TKJobHistoryStatus? Status { get; set; }

        /// <summary></summary>
        public string Summary { get; set; }

        /// <summary></summary>
        public DateTimeOffset Timestamp { get; set; }
    }
}
