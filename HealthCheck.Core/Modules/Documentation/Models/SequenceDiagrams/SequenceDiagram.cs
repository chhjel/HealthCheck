using System.Collections.Generic;
using HealthCheck.Core.Modules.Documentation.Abstractions;

namespace HealthCheck.Core.Modules.Documentation.Models.SequenceDiagrams
{
    /// <summary>
    /// Data object created from <see cref="ISequenceDiagramService"/>
    /// </summary>
    public class SequenceDiagram
    {
        /// <summary>
        /// Name of the diagram.
        /// </summary>
        public string Name { get; set; }

        ///// <summary>
        ///// Description of the diagram.
        ///// </summary>
        //public string Description { get; set; }

        /// <summary>
        /// All the steps within the diagram.
        /// </summary>
        public List<SequenceDiagramStep> Steps { get; set; } = new List<SequenceDiagramStep>();

        /// <summary>
        /// Remarks if any.
        /// </summary>
        public List<SequenceDiagramRemark> Remarks { get; set; } = new List<SequenceDiagramRemark>();
    }
}
