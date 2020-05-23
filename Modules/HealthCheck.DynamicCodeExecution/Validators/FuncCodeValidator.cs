using HealthCheck.DynamicCodeExecution.Abstractions;
using HealthCheck.DynamicCodeExecution.Models;
using System;

namespace HealthCheck.DynamicCodeExecution.Validators
{
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

        public DynamicCodeValidationResult Validate(string code) => Func(code);
    }
}
