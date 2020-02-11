using System.Collections.Generic;
using System.Reflection;

namespace HealthCheck.Core.Modules.Diagrams.SequenceDiagrams
{
    internal class SequenceDiagramAttributeGroup
    {
        public string DiagramId { get; set; }
        public MethodInfo Method { get; set; }
        public List<SequenceDiagramStepAttribute> Attributes { get; set; }
    }
}
