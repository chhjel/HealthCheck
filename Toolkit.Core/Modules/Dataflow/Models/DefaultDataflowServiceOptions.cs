using QoDL.Toolkit.Core.Modules.Dataflow.Abstractions;
using QoDL.Toolkit.Core.Modules.Dataflow.Services;
using System.Collections.Generic;

namespace QoDL.Toolkit.Core.Modules.Dataflow.Models;

/// <summary>
/// Options for <see cref="DefaultDataflowService{TAccessRole}"/>.
/// </summary>
public class DefaultDataflowServiceOptions<TAccessRole>
{
    /// <summary>
    /// Streams that returns data to display.
    /// </summary>
    public IEnumerable<IDataflowStream<TAccessRole>> Streams { get; set; }

    /// <summary>
    /// Optional searches across streams.
    /// </summary>
    public IEnumerable<ITKDataflowUnifiedSearch<TAccessRole>> UnifiedSearches { get; set; }
}
