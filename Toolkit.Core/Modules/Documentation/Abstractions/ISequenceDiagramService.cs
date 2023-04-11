using QoDL.Toolkit.Core.Modules.Documentation.Attributes;
using QoDL.Toolkit.Core.Modules.Documentation.Models.SequenceDiagrams;
using System.Collections.Generic;
using System.Reflection;

namespace QoDL.Toolkit.Core.Modules.Documentation.Abstractions
{
    /// <summary>
    /// Generates sequence diagram data from <see cref="SequenceDiagramStepAttribute"/>s.
    /// </summary>
    public interface ISequenceDiagramService
    {
        /// <summary>
        /// Generates sequence diagram data from <see cref="SequenceDiagramStepAttribute"/>s in the given assemblies.
        /// </summary>
        List<SequenceDiagram> Generate(IEnumerable<Assembly> sourceAssemblies = null);
    }
}