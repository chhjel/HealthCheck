using QoDL.Toolkit.Core.Extensions;
using QoDL.Toolkit.Core.Modules.GoTo.Abstractions;
using QoDL.Toolkit.Core.Modules.GoTo.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QoDL.Toolkit.Dev.Common.GoTo
{
    public class PotatoGotoResolver : ITKGoToResolver
    {
        public string Name => "Potato";

        public Task<TKGoToResolvedData> TryResolveAsync(string input)
        {
            var dummyIds = new[] { "potato", "carrot", "onion", "222" };
            var match = dummyIds.FirstOrDefault(x => x == input);
            if (match == null) return Task.FromResult<TKGoToResolvedData>(null);
            return Task.FromResult(new TKGoToResolvedData
            {
                Name = $"Potato {match.SpacifySentence()}",
                Description = $"This one is a {match}.",
                ResolvedFrom = "DummyId",
                Urls = new List<TKGoToResolvedUrl> { new TKGoToResolvedUrl("Some link", $"/potato/{match}") }
            });
        }
    }
}
