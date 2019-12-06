﻿using HealthCheck.Core.Modules.RequestLog.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HealthCheck.RequestLog.Abstractions
{
    /// <summary>
    /// Stores test log entries.
    /// </summary>
    public interface IRequestLogStorage
    {
        /// <summary>
        /// Insert a new item or update existing with the entry id.
        /// </summary>
        void InsertOrUpdate(LoggedEndpointDefinition entry);

        /// <summary>
        /// Insert a new item.
        /// </summary>
        void Insert(LoggedEndpointDefinition entry);

        /// <summary>
        /// Get the first entry with the given endpoint id.
        /// </summary>
        LoggedEndpointDefinition GetEntryWithEndpointId(string endpointId);

        /// <summary>
        /// Clear all litems.
        /// </summary>
        Task ClearAll();

        /// <summary>
        /// Get all items.
        /// </summary>
        List<LoggedEndpointDefinition> GetAll();
    }
}