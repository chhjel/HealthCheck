using QoDL.Toolkit.Core.Modules.GoTo.Abstractions;

namespace QoDL.Toolkit.Core.Modules.GoTo
{
    /// <summary>
    /// Options for <see cref="TKGoToModule"/>.
    /// </summary>
    public class TKGoToModuleOptions
    {
        /// <summary>
        /// Service that handles the resolve management.
        /// </summary>
        public ITKGoToService Service { get; set; }
    }
}
