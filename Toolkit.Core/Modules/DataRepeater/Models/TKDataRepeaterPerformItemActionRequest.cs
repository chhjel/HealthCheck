using System;
using System.Collections.Generic;

namespace QoDL.Toolkit.Core.Modules.DataRepeater.Models
{
    /// <summary></summary>
    public class TKDataRepeaterPerformItemActionRequest
    {
        /// <summary>
        /// Type of the stream.
        /// </summary>
        public string StreamId { get; set; }

        /// <summary>
        /// Type of the action.
        /// </summary>
        public string ActionId { get; set; }

        /// <summary></summary>
        public Guid ItemId { get; set; }

        /// <summary></summary>
        public Dictionary<string, string> Parameters { get; set; }
    }
}
