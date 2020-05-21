﻿using HealthCheck.Core.Modules.Documentation.Abstractions;

namespace HealthCheck.Core.Modules.Documentation
{
    /// <summary>
    /// Options for <see cref="HCDocumentationModule"/>.
    /// </summary>
    public class HCDocumentationModuleOptions
    {
        /// <summary>
        /// Handles sequence diagrams.
        /// </summary>
        public ISequenceDiagramService SequenceDiagramService { get; set; }

        /// <summary>
        /// Handles flow charts.
        /// </summary>
        public IFlowChartsService FlowChartsService { get; set; }

        /// <summary>
        /// If true then diagram data will only be retrieved from services once and cached in memory.
        /// </summary>
        public bool CacheDiagrams { get; set; } = true;
    }
}
