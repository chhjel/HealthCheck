using QoDL.Toolkit.Core.Modules.Comparison.Abstractions;
using QoDL.Toolkit.Core.Modules.Comparison.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static QoDL.Toolkit.Dev.Common.Comparison.DummyComparisonTypeHandler;

namespace QoDL.Toolkit.Dev.Common.Comparison;

public class DummyComparisonTypeHandler : TKComparisonTypeHandlerBase<DummyThing>
{
    public override string Description => "Dummy things included.";

    private readonly List<DummyThing> _items = Enumerable.Range(1, 100)
        .Select(x => new DummyThing
        {
            Id = x,
            Name = $"Dummy #{x}",
            Description = $"Some description here and a number {x}"
        })
        .ToList();

    public override Task<List<TKComparisonInstanceSelection>> GetFilteredOptionsAsync(TKComparisonTypeFilter filter)
    {
        var items = _items
            .Where(x => x.Id.ToString().Contains(filter.Input))
            .Take(10)
            .Select(x => new TKComparisonInstanceSelection
            {
                Id = x.Id.ToString(),
                Name = x.Name,
                Description = null
            })
            .ToList();
        return Task.FromResult(items);
    }

    public override Task<DummyThing> GetInstanceWithIdOfAsync(string id)
        => Task.FromResult(_items.FirstOrDefault(x => x.Id.ToString() == id));

    public override string GetInstanceDisplayNameOf(DummyThing instance) => instance.Name;

    public class DummyThing
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }
}
