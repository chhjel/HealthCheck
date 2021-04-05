namespace HealthCheck.Core.Modules.Tests.Models
{
    /// <summary></summary>
    public class HCTestsJsonTemplateResult
    {
        /// <summary></summary>
        internal bool NoTemplate { get; private set; }

        /// <summary></summary>
        internal object Instance { get; private set; }

        /// <summary>
        /// A serialized template will be created from the given instance.
        /// </summary>
        public static HCTestsJsonTemplateResult CreateTemplateFrom(object instance) => new() { Instance = instance };

        /// <summary>
        /// Do not create any template for this type.
        /// </summary>
        public static HCTestsJsonTemplateResult CreateNoTemplate() => new() { NoTemplate = true };

        /// <summary>
        /// Use default built in logic, a new instance will be activated and serialized as a template.
        /// </summary>
        public static HCTestsJsonTemplateResult CreateDefault() => new() { };
    }
}
