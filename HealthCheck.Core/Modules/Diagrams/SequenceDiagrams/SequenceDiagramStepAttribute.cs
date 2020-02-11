using System;
using System.Collections.Generic;
using System.Linq;

namespace HealthCheck.Core.Modules.Diagrams.SequenceDiagrams
{
    [AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
    public class SequenceDiagramStepAttribute : Attribute
    {
        public string DiagramId { get; private set; }
        internal List<string> Branches { get; private set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Note { get; set; }
        public string Remarks { get; set; }
        public string NextId { get; set; }
        public string OptionalId { get; set; }
        internal string UICategory { get; set; }

        internal string ClassName { get; set; }
        internal string MethodName { get; set; }
        internal SequenceDiagramStepAttribute Next { get; set; }
        internal SequenceDiagramStepAttribute Previous { get; set; }

        public SequenceDiagramStepAttribute(
            string diagramId,
            string name,
            string description = null,
            string note = null,
            string remarks = null,
            string next = null,
            string[] branches = null,
            string optionalId = null,
            string uiCategory = null,
            bool onlyIncludeInBranches = false
        )
        {
            DiagramId = diagramId;
            Branches = branches?.ToList() ?? new List<string>();
            if (!onlyIncludeInBranches)
            {
                Branches.Add(diagramId);
            }
            Name = name;
            Description = description;
            Note = note;
            Remarks = remarks;
            NextId = next;
            OptionalId = optionalId;
            UICategory = uiCategory;
        }

        internal void SetNext(SequenceDiagramStepAttribute other)
        {
            Next = other;
            other.Previous = this;
        }

        internal void SetPrevious(SequenceDiagramStepAttribute other)
        {
            Previous = other;
            other.Next = this;
        }
    }
}
