using QoDL.Toolkit.Core.Models;
using QoDL.Toolkit.Core.Modules.Tests.Models;
using System;

namespace QoDL.Toolkit.Core.Modules.Tests.Attributes;

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
    /// Method name of a public static method in the same class as this method. The method should have the same return type as this parameter or <see cref="TKDefaultTestParameterValue{T}"/> and have zero parameters or one string parameter.
    /// <para>If the method has one string parameter, the name of the parameter will be its value.</para>
    /// </summary>
    public string DefaultValueFactoryMethod { get; set; }

    /// <summary>
    /// Hint of how to display a parameter input.
    /// </summary>
    public TKUIHint UIHints { get; set; }

    /// <summary>
    /// Use to override the label/placeholder/name displayed for any null-value.
    /// </summary>)
    public string NullName { get; set; }

    /// <summary>
    /// Can be used on text inputs to require the input to match the given regex pattern.
    /// <para>Use JavaScript format. If not starting with a '/' one will be prepended and '/g' will be appended.</para>
    /// <para>Example: O\-\d+ or /[abc]+/gi</para>
    /// </summary>
    public string TextPattern { get; set; }

    /// <summary>
    /// If <see cref="UIHints"/> include <see cref="TKUIHint.CodeArea"/>, this can be set to 'csharp', 'json', 'xml' or 'sql' to give the editor a hint of what content is displayed.
    /// <para>Defaults to 'json'</para>
    /// </summary>
    public string CodeLanguage { get; set; } = "json";

    /// <summary>
    /// Sets parameters options.
    /// </summary>
    /// <param name="target">Target parameter name.</param>
    /// <param name="name">New name of the property.</param>
    /// <param name="description">Description text that will be visible as a help text.</param>
    /// <param name="uiHints">Optional hints for display options.</param>
    /// <param name="nullName">Use to override the label/placeholder/name displayed for any null-value.</param>
    public RuntimeTestParameterAttribute(string target, string name, string description, TKUIHint uiHints = TKUIHint.None, string nullName = null)
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
    public RuntimeTestParameterAttribute(string name = null, string description = null, TKUIHint uIHints = TKUIHint.None, string nullName = null)
    {
        Name = name;
        Description = description;
        UIHints = uIHints;
        NullName = nullName;
    }

    /// <summary>Deprecated, use TKUIHint instead.</summary>
    [Flags]
    [Obsolete("Use QoDL.Toolkit.Core.Models.TKUIHint instead.")]
    public enum UIHint
    {
        /// <summary></summary>
        None = 0,
        /// <summary></summary>
        NotNull = 1,
        /// <summary></summary>
        TextArea = 2,
        /// <summary></summary>
        ReadOnlyList = 4,
        /// <summary></summary>
        FullWidth = 8,
        /// <summary></summary>
        CodeArea = 16
    }
}
