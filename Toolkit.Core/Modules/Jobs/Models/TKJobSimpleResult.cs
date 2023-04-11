namespace QoDL.Toolkit.Core.Modules.Jobs.Models;

/// <summary></summary>
public class TKJobSimpleResult
{
    /// <summary></summary>
    public bool Success { get; set; }
    /// <summary></summary>
    public string Message { get; set; }

    /// <summary></summary>
    public static TKJobSimpleResult CreateSuccess(string message = null)
        => new() { Success = true, Message = message };

    /// <summary></summary>
    public static TKJobSimpleResult CreateError(string message)
        => new() { Message = message };
}
