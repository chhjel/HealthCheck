using HealthCheck.Core.Modules.ActionsTestLog.Models;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace HealthCheck.Core.Abstractions
{
    /// <summary>
    /// Handles the request testlog.
    /// </summary>
    public interface ITestLogService
    {
        /// <summary>
        /// Handle a new event.
        /// </summary>
        void HandleActionEvent(LogFilterEvent e);

        /// <summary>
        /// Create an id for the given endpoint.
        /// </summary>
        string CreateEndpointId(Type controllerType, MethodInfo actionMethod);

        /// <summary>
        /// Clear all stored actions.
        /// </summary>
        Task ClearActions();

        /// <summary>
        /// Store the given entry.
        /// </summary>
        void StoreAction(LoggedActionEntry entry);

        /// <summary>
        /// Get all stored actions.
        /// </summary>
        List<LoggedActionEntry> GetActions();

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