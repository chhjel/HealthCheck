using HealthCheck.Core.Modules.Dataflow.Abstractions;

namespace HealthCheck.Core.Modules.Dataflow
{
    /// <summary>
    /// Options for <see cref="HCDataflowModule{TAccessRole}"/>.
    /// </summary>
    public class HCDataflowModuleOptions<TAccessRole>
    {
        /// <summary>
        /// Must be set for the dataflow tab to be shown.
        /// </summary>
        public IDataflowService<TAccessRole> DataflowService { get; set; }
    }
}
