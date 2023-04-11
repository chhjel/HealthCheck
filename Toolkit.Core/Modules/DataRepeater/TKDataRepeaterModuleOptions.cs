using QoDL.Toolkit.Core.Modules.DataRepeater.Abstractions;

namespace QoDL.Toolkit.Core.Modules.DataRepeater
{
    /// <summary>
    /// Options for <see cref="TKDataRepeaterModule"/>.
    /// </summary>
    public class TKDataRepeaterModuleOptions
    {
        /// <summary>
        /// Service that handles the repeating.
        /// </summary>
        public ITKDataRepeaterService Service { get; set; }
    }
}
