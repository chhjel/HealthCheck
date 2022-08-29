using HealthCheck.Core.Modules.Comparison.Abstractions;
using HealthCheck.Core.Modules.Comparison.Models;
using System.Threading.Tasks;

namespace HealthCheck.Dev.Common.Comparison
{
    public class DevOrderDifferA : HCComparisonDifferBase<DevOrderComparisonTypeHandler.CmpOrder>
    {
        public override string Name => "Some other diff";

        public override Task<HCComparisonDifferOutput> CompareInstancesAsync(DevOrderComparisonTypeHandler.CmpOrder left, DevOrderComparisonTypeHandler.CmpOrder right, string leftName, string rightName)
        {
            return Task.FromResult(
                new HCComparisonDifferOutput()
                    .AddNote($"This is a note.", "Note test")
                    .AddSideNotes($"This ones name is '{leftName}'", $"And this ones name is '{rightName}'", "Side note test")
                    .AddHtml($"This is <b>HTML</b>.", "Html test")
                    .AddSideHtml($"This ones name is <b>'{leftName}'</b>", $"And this ones name is <b>'{rightName}'</b>", "Side html test")
            );
        }
    }
}
