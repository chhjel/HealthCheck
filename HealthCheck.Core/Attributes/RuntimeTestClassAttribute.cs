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
        /// Test set id. Defaults to the full class typename if empty.
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
        public bool DefaultAllowParallelExecution { get; set; }

        /// <summary>
        /// If enabled the test in this class can be executed from the ui manually by default.
        /// <para>True by default.</para>
        /// </summary>
        public bool DefaultAllowManualExecution { get; set; } = true;

        /// <summary>
        /// Default roles that are allowed access to the tests in this class.
        /// <para>Must be an enum flags value.</para>
        /// </summary>
        public object DefaultRolesWithAccess { get; set; }

        /// <summary>
        /// Optional group name.
        /// </summary>
        public string GroupName { get; set; }

        /// <summary>
        /// Optional icon name override. Get name from https://material.io/tools/icons/.
        /// </summary>
        public string Icon { get; set; }

        /// <summary>
        /// Order of the set in the ui lists, higher value = higher up.
        /// <para>If groups are used this is the order within its group.</para>
        /// </summary>
        public int UIOrder { get; set; }
    }
}
