using QoDL.Toolkit.RequestLog.Models;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace QoDL.Toolkit.RequestLog.Abstractions
{
    /// <summary>
    /// Handles request log data.
    /// </summary>
    public interface IRequestLogService
    {
        /// <summary>
        /// Handle a new event.
        /// </summary>
        void HandleRequestEvent(LogFilterEvent e);

        /// <summary>
        /// Create an id for the given endpoint.
        /// </summary>
        string CreateEndpointId(Type controllerType, MethodInfo actionMethod, string actionName);

        /// <summary>
        /// Clear all calls/errors, and optionally definitions.
        /// </summary>
        Task ClearRequests(bool includeDefinitions = false);

        /// <summary>
        /// Store the given entry.
        /// </summary>
        void StoreRequest(LoggedEndpointDefinition entry);

        /// <summary>
        /// Get all stored actions.
        /// </summary>
        List<LoggedEndpointDefinition> GetRequests();

        /// <summary>
        /// Get the current application version.
        /// </summary>
        string GetCurrentVersion();

        /// <summary>
        /// Get a func that creates group names from the given controller type, or null for no grouping.
        /// </summary>
        Func<Type, string> GetControllerGroupNameFactory();
    }
}