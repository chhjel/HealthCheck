﻿using System.Collections.Generic;

namespace HealthCheck.Core.Modules.Diagrams.SequenceDiagrams
{
    /// <summary>
    /// A step of a sequence diagra.
    /// </summary>
    public class SequenceDiagramStep
    {
        /// <summary>
        /// Index of the step.
        /// </summary>
        public int Index { get; set; }

        /// <summary>
        /// From name.
        /// </summary>
        public string From { get; set; }

        /// <summary>
        /// To name.
        /// </summary>
        public string To { get; set; }

        /// <summary>
        /// Description of process going from <see cref="From"/> to <see cref="To"/>.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Optional note for this step. Is shown in the diagram.
        /// </summary>
        public string Note { get; set; }

        /// <summary>
        /// Optional remark for this step. Is shown below the diagram.
        /// </summary>
        public string Remarks { get; set; }

        /// <summary>
        /// Number of the remark if any.
        /// </summary>
        public int? RemarkNumber { get; set; }

        /// <summary>
        /// If given this step is an optional part of the process.
        /// <para>Steps next to eachother with the same optional id are grouped together.</para>
        /// </summary>
        public string OptionalId { get; set; }

        /// <summary>
        /// Direction of this step.
        /// </summary>
        public SequenceDiagramStepDirection Direction { get; set; }
        
        /// <summary>
        /// Branches this step is a part of.
        /// </summary>
        public List<string> Branches { get; set; }

        /// <summary>
        /// $"{From} -> {To}: {Description} ({Note})";
        /// </summary>
        public override string ToString() => $"{From} -> {To}: {Description} ({Note})";
    }
}
