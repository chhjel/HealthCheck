using System;
using System.Collections.Generic;

namespace HealthCheck.Core.Modules.EventNotifications
{
    /// <summary>
    /// Provides <see cref="KnownEventDefinition"/>s.
    /// </summary>
    public interface IEventSinkKnownEventDefinitionsStorage
    {
        /// <summary>
        /// Get all definitions.
        /// </summary>
        IEnumerable<KnownEventDefinition> GetDefinitions();

        /// <summary>
        /// Insert the given definition.
        /// </summary>
        KnownEventDefinition InsertDefinition(KnownEventDefinition definition);

        /// <summary>
        /// Updates the given definition.
        /// </summary>
        KnownEventDefinition UpdateDefinition(KnownEventDefinition definition);

        /// <summary>
        /// Delete event definition for the given event id.
        /// </summary>
        void DeleteDefinition(string eventId);

        /// <summary>
        /// Delete all event definitions.
        /// </summary>
        /// <returns>Number of deleted definitions.</returns>
        void DeleteDefinitions();
    }
}
