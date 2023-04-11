using QoDL.Toolkit.Core.Modules.Documentation.Abstractions;
using System.Collections.Generic;
using System.Reflection;

namespace QoDL.Toolkit.Core.Modules.Documentation.Services
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
