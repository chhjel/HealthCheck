using System;

namespace HealthCheck.Core.Attributes
{
    /// <summary>
    /// Classes containing tests must be marked with this attribute.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class RuntimeTestClassAttribute : Attribute
    {
        /// <summary>
        /// Test set id.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Name of the test set. Shown in the UI and included in json result.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Description of the test set. Shown in the UI and included in json result.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// If true the test in this class will be executed in parallel by default.
        /// </summary>
        public bool AllowParallelExecution { get; set; }

        ///// <summary>
        ///// If enabled the test in this class can be executed from the ui manually by default.
        ///// </summary>
        //public bool AllowManualExecution { get; set; }
    }
}
