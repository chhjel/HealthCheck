using HealthCheck.Core.Extensions;
using HealthCheck.Core.Modules.GoTo.Abstractions;
using HealthCheck.Core.Modules.GoTo.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HealthCheck.Dev.Common.GoTo
{
    public class PotatoGotoResolver : IHCGoToResolver
    {
        public string Name => "Potato";

        public Task<HCGoToResolvedData> TryResolveAsync(string input)
        {
            var dummyIds = new[] { "potato", "carrot", "onion", "222" };
            var match = dummyIds.FirstOrDefault(x => x == input);
            if (match == null) return Task.FromResult<HCGoToResolvedData>(null);
            return Task.FromResult(new HCGoToResolvedData
            {
                Name = $"Potato {match.SpacifySentence()}",
                Description = $"This one is a {match}.",
                ResolvedFrom = "DummyId",
                Urls = new List<HCGoToResolvedUrl> { new HCGoToResolvedUrl("Some link", $"/potato/{match}") }
            });
        }
    }
}
