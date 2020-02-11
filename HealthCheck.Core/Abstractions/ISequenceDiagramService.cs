using HealthCheck.Core.Modules.Diagrams.SequenceDiagrams;
using System.Collections.Generic;
using System.Reflection;

namespace HealthCheck.Core.Abstractions
{
    /// <summary>
    /// Generates sequence diagram data from <see cref="SequenceDiagramStepAttribute"/>s.
    /// </summary>
    public interface ISequenceDiagramService
    {
        /// <summary>
        /// Generates sequence diagram data from <see cref="SequenceDiagramStepAttribute"/>s in the given assemblies.
        /// </summary>
        List<SequenceDiagram> Generate(IEnumerable<Assembly> sourceAssemblies);
    }
}