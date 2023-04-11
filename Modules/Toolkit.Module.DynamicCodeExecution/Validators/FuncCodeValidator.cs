using QoDL.Toolkit.Module.DynamicCodeExecution.Abstractions;
using QoDL.Toolkit.Module.DynamicCodeExecution.Models;
using System;

namespace QoDL.Toolkit.Module.DynamicCodeExecution.Validators;

/// <summary>
/// Simple validator that delegates all logic to the given func.
/// </summary>
public class FuncCodeValidator : IDynamicCodeValidator
{
    private Func<string, DynamicCodeValidationResult> Func { get; set; }

    /// <summary>
    /// Simple validator that delegates all logic to the given func.
    /// </summary>
    /// <param name="func">Input string is the source code, return is the validation result.</param>
    public FuncCodeValidator(Func<string, DynamicCodeValidationResult> func)
    {
        Func = func;
    }

    /// <summary>
    /// Checks if the code is allowed to execute using the given Func.
    /// </summary>
    public DynamicCodeValidationResult Validate(string code) => Func(code);
}
