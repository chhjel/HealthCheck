using QoDL.Toolkit.RequestLog.Abstractions;

namespace QoDL.Toolkit.Core.Modules.Settings;

/// <summary>
/// Options for <see cref="TKRequestLogModule"/>.
/// </summary>
public class TKRequestLogModuleOptions
{
    /// <summary>
    /// Service used to get and store requests.
    /// </summary>
    public IRequestLogService RequestLogService { get; set; }
}
