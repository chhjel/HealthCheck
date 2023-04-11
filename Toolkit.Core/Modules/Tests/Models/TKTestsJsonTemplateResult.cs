namespace QoDL.Toolkit.Core.Modules.Tests.Models
{
    /// <summary></summary>
    public class TKTestsJsonTemplateResult
    {
        /// <summary></summary>
        internal bool NoTemplate { get; private set; }

        /// <summary></summary>
        internal object Instance { get; private set; }

        /// <summary>
        /// A serialized template will be created from the given instance.
        /// </summary>
        public static TKTestsJsonTemplateResult CreateTemplateFrom(object instance) => new() { Instance = instance };

        /// <summary>
        /// Do not create any template for this type.
        /// </summary>
        public static TKTestsJsonTemplateResult CreateNoTemplate() => new() { NoTemplate = true };

        /// <summary>
        /// Use default built in logic, a new instance will be activated and serialized as a template.
        /// </summary>
        public static TKTestsJsonTemplateResult CreateDefault() => new() { };
    }
}
