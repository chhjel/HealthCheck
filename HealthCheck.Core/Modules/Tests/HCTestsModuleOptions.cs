using System.Collections.Generic;
using System.Reflection;

namespace HealthCheck.Core.Modules.Tests
{
    /// <summary>
    /// Options for <see cref="HCTestsModule"/>.
    /// </summary>
    public class HCTestsModuleOptions
    {
        /// <summary>
        /// The assemblies that contains the test methods.
        /// </summary>
        public IEnumerable<Assembly> AssembliesContainingTests { get; set; }
    }
}
