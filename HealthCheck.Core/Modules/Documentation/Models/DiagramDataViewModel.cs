using HealthCheck.Core.Modules.Documentation.Models.FlowCharts;
using HealthCheck.Core.Modules.Documentation.Models.SequenceDiagrams;
using System.Collections.Generic;

namespace HealthCheck.Core.Modules.Documentation.Models
{
    /// <summary>
    /// Includes data about diagrams
    /// </summary>
    public class DiagramDataViewModel
    {
        /// <summary>
        /// List of sequence diagrams.
        /// </summary>
        public List<SequenceDiagram> SequenceDiagrams { get; set; } = new List<SequenceDiagram>();

        /// <summary>
        /// List of flow charts.
        /// </summary>
        public List<FlowChart> FlowCharts { get; set; } = new List<FlowChart>();
    }
}
