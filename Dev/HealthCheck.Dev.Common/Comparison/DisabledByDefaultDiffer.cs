using HealthCheck.Core.Modules.Comparison.Abstractions;
using HealthCheck.Core.Modules.Comparison.Models;
using System.Threading.Tasks;

namespace HealthCheck.Dev.Common.Comparison
{
    public class DisabledByDefaultDiffer : IHCComparisonDiffer
    {
        public string Name => "Disabled by default";
        public string Description => string.Empty;
        public int UIOrder => 0;
        public bool DefaultEnabledFor(IHCComparisonTypeHandler handler) => handler is DummyComparisonTypeHandler;
        public bool CanHandle(IHCComparisonTypeHandler handler) => true;

        public Task<HCComparisonDifferOutput> CompareInstancesAsync(object left, object right, string leftName, string rightName)
        {
            return Task.FromResult(
                new HCComparisonDifferOutput()
                    .AddNote($"From disabled by default differ.", "Note")
            );
        }
    }
}
