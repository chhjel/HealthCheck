using QoDL.Toolkit.Core.Modules.Documentation.Attributes;
using System.Collections.Generic;
using System.Reflection;

namespace QoDL.Toolkit.Core.Modules.Documentation.Models.SequenceDiagrams;

internal class SequenceDiagramAttributeGroup
{
    public string DiagramId { get; set; }
    public MethodInfo Method { get; set; }
    public List<SequenceDiagramStepAttribute> Attributes { get; set; }
}
