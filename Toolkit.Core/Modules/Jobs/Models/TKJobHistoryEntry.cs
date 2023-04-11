using System;
using System.Collections.Generic;

namespace QoDL.Toolkit.Core.Modules.Jobs.Models
{
    /// <summary>
    /// 
    /// </summary>
    public class TKJobHistoryEntry
    {
        /// <summary></summary>
        public Guid Id { get; set; }

        /// <summary></summary>
        public string SourceId { get; set; }

        /// <summary></summary>
        public string JobId { get; set; }

        /// <summary></summary>
        public DateTimeOffset EndedAt { get; set; }

        /// <summary></summary>
        public Guid? DetailId { get; set; }

        /// <summary></summary>
        public TKJobHistoryStatus Status { get; set; }

        /// <summary></summary>
        public string Summary { get; set; }

        /// <summary></summary>
        public DateTimeOffset? StartedAt { get; set; }
    }
}
