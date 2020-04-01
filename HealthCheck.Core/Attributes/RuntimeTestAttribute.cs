using HealthCheck.Core.Entities;
using System;

namespace HealthCheck.Core.Attributes
{
    /// <summary>
    /// Decorate methods with this attribute to mark them as a test that can be executed at runtime.
    /// <para>Method must be static.</para>
    /// <para>Method must return a <see cref="TestResult"/> (use the TestResult.Create.. methods).</para>
    /// <para>Method parameters should have default values.</para>
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class RuntimeTestAttribute : Attribute
    {
        /// <summary>
        /// Decorate methods with this attribute to mark them as a test that can be executed at runtime.
        /// <para>Method must be public.</para>
        /// <para>Parent class must be decorated with <see cref="RuntimeTestClassAttribute"/>.</para>
        /// <para>Method must return a <see cref="TestResult"/> (use the TestResult.Create.. methods) and can be async.</para>
        /// <para>Method parameters should have default values.</para>
        /// </summary>
        public RuntimeTestAttribute(string name = null, string description = null)
        {
            Name = name;
            Description = description;
        }

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
        /// Optional category that can be filtered upon. Will be unioned with <see cref="Categories"/>.
        /// <para>If set it will override any default categories set on the class.</para>
        /// </summary>
        public string Category { get; set; }

        /// <summary>
        /// Optional categories that can be filtered upon. Will be unioned with <see cref="Category"/>.
        /// <para>If set it will override any default categories set on the class.</para>
        /// </summary>
        public string[] Categories { get; set; }

        /// <summary>
        /// Set roles that are allowed access to the tests in this class.
        /// <para>Must be an enum flags value.</para>
        /// </summary>
        public object RolesWithAccess { get; set; }

        /// <summary>
        /// Text on the button when the check is not executing.
        /// <para>Defaults to "Run"</para>
        /// </summary>
        public string RunButtonText { get; set; }

        /// <summary>
        /// Text on the button when the check is executing.
        /// <para>Defaults to "Runnings.."</para>
        /// </summary>
        public string RunningButtonText { get; set; }
    }
}
