namespace HealthCheck.Core.Modules.Tests.Models
{
    /// <summary>
    /// A possible choice to select from in class proxy tests.
    /// </summary>
    public class RuntimeTestReferenceParameterChoice
    {
        /// <summary>
        /// Id of the choice that will be fed back to <see cref="RuntimeTestReferenceParameterFactory._getInstanceFromIdFactory"/>
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Label for the choice that will be visible in frontend.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Optional description that will be visible in frontend.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// A possible choice to select from in class proxy tests.
        /// </summary>
        public RuntimeTestReferenceParameterChoice(string id, string name, string description = null)
        {
            Id = id;
            Name = name;
            Description = description;
        }
    }
}
