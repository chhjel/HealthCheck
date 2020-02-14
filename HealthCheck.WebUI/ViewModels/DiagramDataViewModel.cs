using HealthCheck.Core.Modules.Diagrams.SequenceDiagrams;
using System.Collections.Generic;

namespace HealthCheck.WebUI.ViewModels
{
    /// <summary>
    /// Includes data about diagrams
    /// </summary>
    public class DiagramDataViewModel
    {
        /// <summary>
        /// List of test sets.
        /// </summary>
        public List<SequenceDiagram> SequenceDiagrams { get; set; } = new List<SequenceDiagram>();
    }
}
