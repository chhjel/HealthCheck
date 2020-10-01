using static HealthCheck.Core.Modules.Tests.Models.ClassProxyRuntimeTestConfig;

namespace HealthCheck.Core.Modules.Tests.Models
{
    /// <summary>
    /// A possible choice to select from in class proxy tests.
    /// </summary>
    public class ClassProxyRuntimeTestParameterChoice
    {
        /// <summary>
        /// Id of the choice that will be fed back to <see cref="ClassProxyRuntimeTestParameterFactory.GetInstanceFromIdFactory"/>
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Label for the choice that will be visible in frontend.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// A possible choice to select from in class proxy tests.
        /// </summary>
        public ClassProxyRuntimeTestParameterChoice(string id, string name)
        {
            Id = id;
            Name = name;
        }
    }
}
