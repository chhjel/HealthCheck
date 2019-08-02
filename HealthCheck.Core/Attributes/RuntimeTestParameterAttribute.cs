using System;

namespace HealthCheck.Core.Attributes
{
    /// <summary>
    /// Set parameter options by either:
    /// <para>* Decorating parameters directly.</para>
    /// <para>* Decorating methods and use the <see cref="Target"/> property to select what parameter to target by its name.</para>
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class RuntimeTestParameterAttribute : Attribute
    {
        /// <summary>
        /// Target property name. Only used if you placed this attribute on a method and not the parameter itself.
        /// </summary>
        public string Target { get; set; }

        /// <summary>
        /// Name of the property
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Description of the property. Shown as a help text and can contain html.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Do not allow null-values to be entered in the user interface. Does not affect nullable parameters.
        /// </summary>
        public bool NotNull { get; set; }

        /// <summary>
        /// Only affects generic lists. Does not allow new entries to be added, or existing entries to be changed.
        /// </summary>
        public bool ReadOnlyList { get; set; }

        /// <summary>
        /// Method name of a public static method in the same class as this method. The method should have the same return type as this parameter, and have zero parameters.
        /// </summary>
        public string DefaultValueFactoryMethod { get; set; }

        /// <summary>
        /// Sets parameters options.
        /// </summary>
        /// <param name="target">Target parameter name.</param>
        /// <param name="name">New name of the property.</param>
        /// <param name="description">Description text that will be visible as a help text.</param>
        /// <param name="notNull">If true null-values will not be allowed to be entered in the user interface. Does not affect nullable parameters.</param>
        /// <param name="readOnlyList">Only affects generic lists. Does not allow new entries to be added, or existing entries to be changed.</param>
        public RuntimeTestParameterAttribute(string target, string name, string description, bool notNull = false, bool readOnlyList = false)
        {
            Target = target;
            Name = name;
            Description = description;
            NotNull = notNull;
            ReadOnlyList = readOnlyList;
        }

        /// <summary>
        /// Sets parameters options.
        /// <para>Do not use this constructor if decorating the method itself.</para>
        /// </summary>
        /// <param name="name">New name of the property.</param>
        /// <param name="description">Description text that will be visible as a help text.</param>
        /// <param name="notNull">If true null-values will not be allowed to be entered in the user interface. Does not affect nullable parameters.</param>
        /// <param name="readOnlyList">Only affects generic lists. Does not allow new entries to be added, or existing entries to be changed. Entries can only be sorted.</param>
        public RuntimeTestParameterAttribute(string name = null, string description = null, bool notNull = false, bool readOnlyList = false)
        {
            Name = name;
            Description = description;
            NotNull = notNull;
            ReadOnlyList = readOnlyList;
        }

        /// <summary>
        /// Set parameter options by either:
        /// <para>* Decorating parameters directly.</para>
        /// <para>* Decorating methods and use the <see cref="Target"/> property to select what parameter to target by its name.</para>
        /// </summary>
        public RuntimeTestParameterAttribute() { }
    }
}
