using HealthCheck.Core.Modules.ContentPermutation.Abstractions;
using HealthCheck.Core.Modules.ContentPermutation.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HealthCheck.Dev.Common.ContentPermutation
{
    public class DevProductPermutationHandler : HCContentPermutationContentHandlerBase<ProductPermutations>
	{
		private readonly List<DevProduct> _all = new() {
			new DevProduct { Index = 0, Details = "Product A", IsExpired = false, IsOnSale = false, Type = ProductPermutations.ProductType.Product },
			new DevProduct { Index = 1, Details = "Package A", IsExpired = false, IsOnSale = false, Type = ProductPermutations.ProductType.Package },
			new DevProduct { Index = 2, Details = "Bundle A", IsExpired = false, IsOnSale = false, Type = ProductPermutations.ProductType.Bundle },
			new DevProduct { Index = 3, Details = "Product B", IsExpired = true, IsOnSale = false, Type = ProductPermutations.ProductType.Product },
			new DevProduct { Index = 4, Details = "Product C", IsExpired = true, IsOnSale = true, Type = ProductPermutations.ProductType.Product },
			new DevProduct { Index = 5, Details = "Product D", IsExpired = false, IsOnSale = true, Type = ProductPermutations.ProductType.Product },
		};

		public override Task<List<HCPermutatedContentItemViewModel>> GetContentForAsync(ProductPermutations permutation)
		{
			var matches = _all.Where(x => x.IsExpired == permutation.IsExpired && x.IsOnSale == permutation.IsOnSale && x.Type == permutation.Type);
			var models = matches.Select(x => new HCPermutatedContentItemViewModel(x.Details, $"/item_{x.Index}")
			{
				AdditionalUrls = new List<HCPermutatedContentLinkViewModel>
				{
					new HCPermutatedContentLinkViewModel("Details here for WebAdmins", "/details_here") {
						AccessRoles = RuntimeTestAccessRole.WebAdmins
					},
					new HCPermutatedContentLinkViewModel("Details here for SystemAdmins", "/details_here") {
						AccessRoles = RuntimeTestAccessRole.SystemAdmins
					},
					new HCPermutatedContentLinkViewModel("Details here for anyone", "/details_here")
				},
				Description = $"{x.Type} description #{x.Index} here.",
				ImageUrl = "https://picsum.photos/200"
			}).ToList();
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
}
