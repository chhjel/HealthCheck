using QoDL.Toolkit.Core.Modules.Comparison.Abstractions;
using QoDL.Toolkit.Core.Modules.Comparison.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace QoDL.Toolkit.Dev.Common.Comparison;

public class DevOrderDifferA : TKComparisonDifferBase<DevOrderComparisonTypeHandler.CmpOrder>
{
    public override string Name => "Some other diff";

    public override Task<TKComparisonDifferOutput> CompareInstancesAsync(DevOrderComparisonTypeHandler.CmpOrder left, DevOrderComparisonTypeHandler.CmpOrder right, string leftName, string rightName)
    {
        if (left.Id == 2) int.Parse("abc");

        Dictionary<string, string> createDict(DevOrderComparisonTypeHandler.CmpOrder data)
        {
            return new Dictionary<string, string>
            {
                { "Id", left.Id.ToString() },
                { "Name", left.Name },
                { "RngGuid", Guid.NewGuid().ToString() },
                { Guid.NewGuid().ToString(), Guid.NewGuid().ToString() }
            };
        }
        var leftProps = createDict(left);
        var rightProps = createDict(right);


        return Task.FromResult(
            new TKComparisonDifferOutput()
                .AddDictionaryData(leftProps, rightProps, "Dictionary test", leftName, rightName)
                .AddNote($"This is a note.", "Note test")
                .AddSideNotes($"This ones name is '{leftName}'", $"And this ones name is '{rightName}'", "Side note test")
                .AddHtml($"This is <b>HTML</b>.", "Html test")
                .AddSideHtml($"This ones name is <b>'{leftName}'</b>", $"And this ones name is <b>'{rightName}'</b>", "Side html test")
        );
    }
}
