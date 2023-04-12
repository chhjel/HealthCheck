using System;

namespace QoDL.Toolkit.RequestLog.Attributes;

/// <summary>
/// Decorate actions with this to set a custom name/description or hide it.
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, Inherited = true)]
public class RequestLogInfoAttribute : Attribute
{
    /// <summary>
    /// Name of the decorated action method or controller.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Description of the decorated action method or controller.
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// Hide the decorated action method or controller.
    /// </summary>
    public bool Hidden { get; set; }

    /// <summary>
    /// Optionally force this url for display.
    /// </summary>
    public string Url { get; set; }

    /// <summary>
    /// Decorate actions with this to set a custom name/description or hide it.
    /// </summary>
    public RequestLogInfoAttribute(string name = null, string description = null, string url = null, bool hide = false)
    {
        Name = name;
        Description = description;
        Url = url;
        Hidden = hide;
    }
}
