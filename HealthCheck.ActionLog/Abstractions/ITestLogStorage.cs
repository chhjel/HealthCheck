using HealthCheck.Core.Modules.ActionsTestLog.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HealthCheck.ActionLog.Abstractions
{
    /// <summary>
    /// Stores test log entries.
    /// </summary>
    public interface ITestLogStorage
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
