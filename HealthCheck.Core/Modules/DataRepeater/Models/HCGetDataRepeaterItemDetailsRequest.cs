﻿using System;

namespace HealthCheck.Core.Modules.DataRepeater.Models
{
    /// <summary></summary>
    public class HCGetDataRepeaterItemDetailsRequest
    {
        /// <summary>
        /// Type of the stream.
        /// </summary>
        public string StreamId { get; set; }

        /// <summary></summary>
        public Guid ItemId { get; set; }
    }
}
