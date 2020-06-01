using HealthCheck.Core.Abstractions.Modules;
using HealthCheck.Core.Modules.Documentation.Models;
using HealthCheck.Core.Modules.Documentation.Models.FlowCharts;
using HealthCheck.Core.Modules.Documentation.Models.SequenceDiagrams;
using System;
using System.Collections.Generic;

namespace HealthCheck.Core.Modules.Documentation
{
    /// <summary>
    /// Module for viewing documentation.
    /// </summary>
    public class HCDocumentationModule : HealthCheckModuleBase<HCDocumentationModule.AccessOption>
    {
        private HCDocumentationModuleOptions Options { get; }
        private static DiagramDataViewModel DiagramDataViewModelCache { get; set; }

        /// <summary>
        /// Module for viewing documentation.
        /// </summary>
        public HCDocumentationModule(HCDocumentationModuleOptions options)
        {
            Options = options;
        }

        /// <summary>
        /// Get frontend options for this module.
        /// </summary>
        public override object GetFrontendOptionsObject(HealthCheckModuleContext context) => new
        {
            EnableDiagramSandbox = Options.EnableDiagramSandbox,
            EnableDiagramDetails = Options.EnableDiagramDetails
        };

        /// <summary>
        /// Get config for this module.
        /// </summary>
        public override IHealthCheckModuleConfig GetModuleConfig(HealthCheckModuleContext context) => new HCDocumentationModuleConfig();
        
        /// <summary>
        /// Different access options for this module.
        /// </summary>
        [Flags]
        public enum AccessOption
        {
            /// <summary>Does nothing.</summary>
            Nothing = 0,
        }

        #region Invokable methods
        /// <summary>
        /// Get diagrams to show in the UI.
        /// </summary>
        [HealthCheckModuleMethod]
        public DiagramDataViewModel GetDiagrams()
        {
            if (Options.CacheDiagrams && DiagramDataViewModelCache != null)
            {
                return DiagramDataViewModelCache;
            }

            DiagramDataViewModelCache = new DiagramDataViewModel()
            {
                SequenceDiagrams = Options.SequenceDiagramService?.Generate() ?? new List<SequenceDiagram>(),
                FlowCharts = Options.FlowChartsService?.Generate() ?? new List<FlowChart>()
            };
            return DiagramDataViewModelCache;
        }
        #endregion
    }
}
