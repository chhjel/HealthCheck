using System.Reflection;

namespace HealthCheck.Core.Modules.Tests
{
    /// <summary>
    /// Options for <see cref="HCTestsModule"/>.
    /// </summary>
    public class HCTestsModuleOptions
    {
        /// <summary>
        /// The assembly that contains the test methods. Defaults to entry assembly.
        /// </summary>
        public Assembly AssemblyContainingTests { get; set; }
    }
}
