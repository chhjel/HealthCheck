using QoDL.Toolkit.Core.Modules.Tests.Models;
using System;

namespace QoDL.Toolkit.Core.Util.Models;

/// <summary>
/// Input used by conversion utils.
/// </summary>
public class TKValueInput
{
    /// <summary>
    /// Raw user input.
    /// </summary>
    public string Value { get; set; }

    /// <summary>
    /// Desired output type.
    /// </summary>
    public Type Type { get; set; }

    /// <summary>
    /// True if the input is raw json.
    /// </summary>
    public bool IsJson { get; set; }

    /// <summary>
    /// True if the input is the id of custom reference types to be passed to a factory.
    /// </summary>
    public bool IsCustomReferenceType { get; set; }

    /// <summary>
    /// Factory factory for custom reference types.
    /// </summary>
    public Func<RuntimeTestReferenceParameterFactory> ParameterFactoryFactory { get; set; }
}
