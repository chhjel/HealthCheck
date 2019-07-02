using HealthCheck.Core.Entities;
using System;

namespace HealthCheck.Core.Attributes
{
    /// <summary>
    /// Marks this method as a test that can be executed at runtime.
    /// <para>Method must be static.</para>
    /// <para>Method must return a <see cref="TestResult"/> (use the TestResult.Create.. methods).</para>
    /// <para>Method can have parameters but they must have default values.</para>
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class RuntimeTestAttribute : Attribute
    {
        /// <summary>
        /// Name of the test. Shown in the UI and included in json result.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Description of the test. Shown in the UI and included in json result.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Optional boolean value to override default option on the test class.
        /// </summary>
        public object AllowParallelExecution { get; set; }

        /// <summary>
        /// Optional boolean value to override default option on the test class.
        /// <para>If enabled the test in this class can be executed from the ui manually</para>
        /// </summary>
        public object AllowManualExecution { get; set; } 

        /// <summary>
        /// Names of the paremeters on the method if any. Defaults to their actual names. Shown in the UI.
        /// </summary>
        public string[] ParameterNames { get; set; }

        /// <summary>
        /// Description of the paremeters on the method if any. Shown in the UI.
        /// </summary>
        public string[] ParameterDescriptions { get; set; }

        /// <summary>
        /// Set roles that are allowed access to the tests in this class.
        /// <para>Must be an enum flags value.</para>
        /// </summary>
        public object RolesWithAccess { get; set; }

        /// <summary>
        /// Get the custom parameter name at the given index or null.
        /// </summary>
        public string GetCustomParameterName(int index)
            => (ParameterNames != null && index <= ParameterNames.Length - 1)
            ? ParameterNames[index]
            : null;

        /// <summary>
        /// Get the custom parameter description at the given index or null.
        /// </summary>
        public string GetCustomParameterDescription(int index)
            => (ParameterDescriptions != null && index <= ParameterDescriptions.Length - 1)
            ? ParameterDescriptions[index]
            : null;
    }
}
