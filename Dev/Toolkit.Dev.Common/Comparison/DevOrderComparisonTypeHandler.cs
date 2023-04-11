using QoDL.Toolkit.Core.Modules.Comparison.Abstractions;
using QoDL.Toolkit.Core.Modules.Comparison.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static QoDL.Toolkit.Dev.Common.Comparison.DevOrderComparisonTypeHandler;

namespace QoDL.Toolkit.Dev.Common.Comparison
{
    public class DevOrderComparisonTypeHandler : TKComparisonTypeHandlerBase<CmpOrder>
    {
        public override string Name => "Order";
        public override string Description => "Some description here.";
        public override string FindInstanceDescription => "Find your orders here!";
        public override string FindInstanceSearchPlaceholder => "Search using order id..";

        private readonly List<CmpOrder> _items = Enumerable.Range(1, 1000)
            .Select(x => new CmpOrder
            {
                Id = x,
                Name = $"Order #{x}"
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
                    Description = "Some description here. etc etc etc etc etc etc etc etc etc etc etc etc etc etc etc etc etc etc etc etc etc etc etc etc etc etc etc etc etc etc etc ."
                })
                .ToList();
            return Task.FromResult(items);
        }

        public override Task<CmpOrder> GetInstanceWithIdOfAsync(string id)
            => Task.FromResult(_items.FirstOrDefault(x => x.Id.ToString() == id));

        public override string GetInstanceDisplayNameOf(CmpOrder instance) => instance.Name;

        public class CmpOrder
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }
    }
}
