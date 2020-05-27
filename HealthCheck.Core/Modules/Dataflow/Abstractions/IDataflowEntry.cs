using HealthCheck.Core.Modules.Dataflow.Models;

namespace HealthCheck.Core.Modules.Dataflow.Abstractions
{
    /// <summary>
    /// An entry from <see cref="IDataflowStream{TAccessRole}.GetLatestStreamEntriesAsync(DataflowStreamFilter)"/>.
    /// </summary>
    public interface IDataflowEntry
    {
    }

}
