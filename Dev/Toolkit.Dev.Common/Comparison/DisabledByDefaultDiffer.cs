using QoDL.Toolkit.Core.Modules.Comparison.Abstractions;
using QoDL.Toolkit.Core.Modules.Comparison.Models;
using System.Threading.Tasks;

namespace QoDL.Toolkit.Dev.Common.Comparison;

public class DisabledByDefaultDiffer : ITKComparisonDiffer
{
    public string Name => "Disabled by default";
    public string Description => string.Empty;
    public int UIOrder => 0;
    public bool DefaultEnabledFor(ITKComparisonTypeHandler handler) => handler is DummyComparisonTypeHandler;
    public bool CanHandle(ITKComparisonTypeHandler handler) => true;

    public Task<TKComparisonDifferOutput> CompareInstancesAsync(object left, object right, string leftName, string rightName)
    {
        return Task.FromResult(
            new TKComparisonDifferOutput()
                .AddNote($"From disabled by default differ.", "Note")
        );
    }
}
