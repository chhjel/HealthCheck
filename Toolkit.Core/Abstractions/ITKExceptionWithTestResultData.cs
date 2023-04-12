using QoDL.Toolkit.Core.Modules.Tests.Models;
using System;

namespace QoDL.Toolkit.Core.Abstractions;

/// <summary>
/// Interface that can be added to exceptions to modify results this exception is thrown from.
/// </summary>
public interface ITKExceptionWithTestResultData
{
    /// <summary>
    /// Optional result modification.
    /// </summary>
    public Action<TestResult> ResultModifier { get; }
}
