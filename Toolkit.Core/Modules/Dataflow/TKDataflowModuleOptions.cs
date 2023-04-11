using QoDL.Toolkit.Core.Modules.Dataflow.Abstractions;

namespace QoDL.Toolkit.Core.Modules.Dataflow
{
    /// <summary>
    /// Options for <see cref="TKDataflowModule{TAccessRole}"/>.
    /// </summary>
    public class TKDataflowModuleOptions<TAccessRole>
    {
        /// <summary>
        /// Must be set for the dataflow tab to be shown.
        /// </summary>
        public IDataflowService<TAccessRole> DataflowService { get; set; }
    }
}
