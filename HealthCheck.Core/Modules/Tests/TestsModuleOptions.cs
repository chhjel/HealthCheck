using System.Reflection;

namespace HealthCheck.Core.Modules.Tests
{
    /// <summary>
    /// Options for <see cref="TestsModule"/>.
    /// </summary>
    public class TestsModuleOptions
    {
        /// <summary>
        /// The assembly that contains the test methods. Defaults to entry assembly.
        /// </summary>
        public Assembly AssemblyContainingTests { get; set; }
    }
}
