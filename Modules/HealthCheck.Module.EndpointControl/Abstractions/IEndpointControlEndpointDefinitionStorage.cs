﻿using HealthCheck.Module.EndpointControl.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HealthCheck.Module.EndpointControl.Abstractions
{
    /// <summary>
    /// Stores definition data.
    /// </summary>
    public interface IEndpointControlEndpointDefinitionStorage
    {
        /// <summary>
        /// Checks if a definition for the given endpoint has been stored.
        /// </summary>
        bool HasDefinitionFor(string endpointId);

        /// <summary>
        /// Store a new definition.
        /// </summary>
        void StoreDefinition(EndpointControlEndpointDefinition definition);

        /// <summary>
        /// Get all stored definitions.
        /// </summary>
        IEnumerable<EndpointControlEndpointDefinition> GetDefinitions();

        /// <summary>
        /// Clear all definitions.
        /// </summary>
        Task ClearAllDefinitions();
    }
}
