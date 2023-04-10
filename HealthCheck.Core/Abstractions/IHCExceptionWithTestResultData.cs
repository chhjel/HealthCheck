using HealthCheck.Core.Modules.Tests.Models;
using System;

namespace HealthCheck.Core.Abstractions;

/// <summary>
/// Interface that can be added to exceptions to modify results this exception is thrown from.
/// </summary>
public interface IHCExceptionWithTestResultData
{
    /// <summary>
    /// Optional result modification.
    /// </summary>
    public Action<TestResult> ResultModifier { get; }
}
