using QoDL.Toolkit.Core.Modules.ContentPermutation.Abstractions;
using QoDL.Toolkit.Core.Modules.ContentPermutation.Models;
using QoDL.Toolkit.Core.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QoDL.Toolkit.Dev.Common.ContentPermutation;

public class DevProductPermutationHandler : TKContentPermutationContentHandlerBase<ProductPermutations>
{
    protected override TimeSpan? CacheDuration => TimeSpan.FromSeconds(10);

    private static readonly List<DevProduct> _all = TKPermutationUtils.CreatePermutationsOf<ProductPermutations>()
        .SelectMany((p, i) => Enumerable.Range(0, 10)
            .Select(e => new DevProduct
            {
                Index = (i * 10) + e,
                Details = $"{p.Type} #{(i * 10) + e}",
                IsExpired = p.IsExpired,
                IsOnSale = p.IsOnSale,
                Type = p.Type
            })
        )
        .ToList();

    public override Task<List<TKPermutatedContentItemViewModel>> GetContentForAsync(TKGetContentPermutationContentOptions<ProductPermutations> options)
    {
        var permutation = options.Permutation;
        var matches = _all
            .Where(x => x.IsExpired == permutation.IsExpired && x.IsOnSale == permutation.IsOnSale && x.Type == permutation.Type)
            .Take(options.MaxCount);
        var models = matches
            .Select(x => new TKPermutatedContentItemViewModel(x.Details, $"/item_{x.Index}")
            {
                AdditionalUrls = new List<TKPermutatedContentLinkViewModel>
                {
                        new TKPermutatedContentLinkViewModel("Details here for WebAdmins", "/details_here") {
                            AccessRoles = RuntimeTestAccessRole.WebAdmins
                        },
                        new TKPermutatedContentLinkViewModel("Details here for SystemAdmins", "/details_here") {
                            AccessRoles = RuntimeTestAccessRole.SystemAdmins
                        },
                        new TKPermutatedContentLinkViewModel("Details here for anyone", "/details_here")
                },
                Description = $"{x.Type} description #{x.Index} here.",
                ImageUrl = "https://picsum.photos/200"
            })
            .ToList();
        return Task.FromResult(models);
    }

    public class DevProduct
    {
        public int Index { get; set; }
        public string Details { get; set; }
        public ProductPermutations.ProductType Type { get; set; }
        public bool IsExpired { get; set; }
        public bool IsOnSale { get; set; }
    }
}
