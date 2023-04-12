using QoDL.Toolkit.Core.Modules.GoTo.Abstractions;
using QoDL.Toolkit.Core.Modules.GoTo.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QoDL.Toolkit.Dev.Common.GoTo;

public class ProductGotoResolver : ITKGoToResolver
{
    public string Name => "Product";
    private static int _imgBuster = 0;

    public Task<TKGoToResolvedData> TryResolveAsync(string input)
    {
        if (input == "error") int.Parse("asd");
        var dummyIds = new[] { "111", "222", "333", "444" };
        var match = dummyIds.FirstOrDefault(x => x == input);
        if (match == null) return Task.FromResult<TKGoToResolvedData>(null);
        return Task.FromResult(new TKGoToResolvedData
        {
            Name = $"Product #{match}",
            Description = $"Id: {match}.",
            ImageUrl = $"https://picsum.photos/200/300?c={_imgBuster++}",
            ResolvedFrom = "Code",
            Urls = new List<TKGoToResolvedUrl> {
                new TKGoToResolvedUrl("Somewhere", $"/product/{match}"),
                new TKGoToResolvedUrl("Somewhere else", $"/variant/{match}")
            }
        });
    }
}
