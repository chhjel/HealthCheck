namespace QoDL.Toolkit.Core.Modules.Tests.Models;

/// <summary>
/// Request model sent to <see cref="TKTestsModule.GetReferenceParameterOptions"/>
/// </summary>
public class GetReferenceParameterOptionsRequestModel
{
    /// <summary>
    /// Id of the target test.
    /// </summary>
    public string TestId { get; set; }

    /// <summary>
    /// Index of the target parameter.
    /// </summary>
    public int ParameterIndex { get; set; }

    /// <summary>
    /// Value to filter choices on.
    /// </summary>
    public string Filter { get; set; }
}
