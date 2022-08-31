using HealthCheck.Core.Modules.GoTo.Abstractions;
using HealthCheck.Core.Modules.GoTo.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HealthCheck.Dev.Common.GoTo
{
    public class ProductGotoResolver : IHCGoToResolver
    {
        public string Name => "Product";
        private static int _imgBuster = 0;

        public Task<HCGoToResolvedData> TryResolveAsync(string input)
        {
            if (input == "error") int.Parse("asd");
            var dummyIds = new[] { "111", "222", "333", "444" };
            var match = dummyIds.FirstOrDefault(x => x == input);
            if (match == null) return Task.FromResult<HCGoToResolvedData>(null);
            return Task.FromResult(new HCGoToResolvedData
            {
                Name = $"Product #{match}",
                Description = $"Id: {match}.",
                ImageUrl = $"https://picsum.photos/200/300?c={_imgBuster++}",
                ResolvedFrom = "Code",
                Urls = new List<HCGoToResolvedUrl> {
                    new HCGoToResolvedUrl("Somewhere", $"/product/{match}"),
                    new HCGoToResolvedUrl("Somewhere else", $"/variant/{match}")
                }
            });
        }
    }
}
