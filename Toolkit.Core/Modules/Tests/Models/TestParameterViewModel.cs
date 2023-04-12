using QoDL.Toolkit.Core.Models;
using System.Collections.Generic;

namespace QoDL.Toolkit.Core.Modules.Tests.Models;

/// <summary>
/// View model for a <see cref="TestParameter"/>.
/// </summary>
public class TestParameterViewModel
{
    /// <summary>
    /// Index of the parameter.
    /// </summary>
    public int Index { get; set; }

    /// <summary>
    /// Name of the parameter.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Description of the parameter.
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// Type name of the parameter.
    /// </summary>
    public string Type { get; set; }

    /// <summary>
    /// Stringified default value of the parameter.
    /// </summary>
    public string DefaultValue { get; set; }

    /// <summary>
    /// Values when a selection is possible.
    /// </summary>
    public List<string> PossibleValues { get; set; }

    /// <summary>
    /// Any UIHint flags configured for this parameter.
    /// </summary>
    public List<TKUIHint> UIHints { get; set; }

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
    /// True when a custom parameter factory has been defined for this type.
    /// </summary>
    public bool IsCustomReferenceType { get; set; }

    /// <summary>
    /// If <see cref="UIHints"/> include <see cref="TKUIHint.CodeArea"/>, this can be set to 'csharp', 'json', 'xml' or 'sql' to give the editor a hint of what content is displayed.
    /// <para>Defaults to 'json'</para>
    /// </summary>
    public string CodeLanguage { get; set; } = "json";

    /// <summary>
    /// Used in frontend.
    /// </summary>
    public bool IsUnsupportedJson { get; set; }

    /// <summary>
    /// Config for ReferenceValueFactory if any.
    /// </summary>
    public ReferenceValueFactoryConfigViewModel ReferenceValueFactoryConfig { get; set; }

    /// <summary>
    /// Hide input.
    /// </summary>
    public bool Hidden { get; set; }
}
