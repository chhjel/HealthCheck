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
    }
}
