using System;

namespace HealthCheck.Core.Modules.Tests.Attributes
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
        /// Method name of a public static method in the same class as this method. The method should have the same return type as this parameter, and have zero parameters.
        /// </summary>
        public string DefaultValueFactoryMethod { get; set; }

        /// <summary>
        /// Hint of how to display a parameter input.
        /// </summary>
        public UIHint UIHints { get; set; }

        /// <summary>
        /// Use to override the label/placeholder/name displayed for any null-value.
        /// </summary>)
        public string NullName { get; set; }

        /// <summary>
        /// Sets parameters options.
        /// </summary>
        /// <param name="target">Target parameter name.</param>
        /// <param name="name">New name of the property.</param>
        /// <param name="description">Description text that will be visible as a help text.</param>
        /// <param name="uiHints">Optional hints for display options.</param>
        /// <param name="nullName">Use to override the label/placeholder/name displayed for any null-value.</param>
        public RuntimeTestParameterAttribute(string target, string name, string description, UIHint uiHints = UIHint.None, string nullName = null)
        {
            Target = target;
            Name = name;
            Description = description;
            UIHints = uiHints;
            NullName = nullName;
        }

        /// <summary>
        /// Sets parameters options.
        /// <para>Do not use this constructor if decorating the method itself.</para>
        /// </summary>
        /// <param name="name">New name of the property.</param>
        /// <param name="description">Description text that will be visible as a help text.</param>
        /// <param name="uIHints">Optional hints for display options.</param>
        /// <param name="nullName">Use to override the label/placeholder/name displayed for any null-value.</param>
        public RuntimeTestParameterAttribute(string name = null, string description = null, UIHint uIHints = UIHint.None, string nullName = null)
        {
            Name = name;
            Description = description;
            UIHints = uIHints;
            NullName = nullName;
        }

        /// <summary>
        /// Hint of how to display a parameter input.
        /// </summary>
        [Flags]
        public enum UIHint
        {
            /// <summary>
            /// Default.
            /// </summary>
            None = 0,

            /// <summary>
            /// Null-values will not be allowed to be entered in the user interface. Does not affect nullable parameters.
            /// </summary>
            NotNull = 1,

            /// <summary>
            /// Only affects strings. Shows a multi-line text area instead of a small input field.
            /// </summary>
            TextArea = 2,

            /// <summary>
            /// Only affects generic lists. Does not allow new entries to be added, or existing entries to be changed.
            /// </summary>
            ReadOnlyList = 4,

            /// <summary>
            /// Make the input field full width in size.
            /// </summary>
            FullWidth = 8,

            /// <summary>
            /// Make the input field a full width vscode editor.
            /// </summary>
            CodeArea = 16
        }

    }
}
