using HealthCheck.Core.Abstractions;
using System.Collections.Generic;
using System.Reflection;

namespace HealthCheck.Core.Modules.Diagrams.SequenceDiagrams
{
    /// <summary>
    /// Options for <see cref="DefaultSequenceDiagramService"/>
    /// </summary>
    public class DefaultSequenceDiagramServiceOptions
    {
        /// <summary>
        /// Default assemblies to detect diagram data from if no assemblies are specified in the 
        /// <see cref="ISequenceDiagramService.Generate(IEnumerable{Assembly})"/> method.
        /// </summary>
        public IEnumerable<Assembly> DefaultSourceAssemblies { get; set; }
    }
}
