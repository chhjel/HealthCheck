using QoDL.Toolkit.Core.Modules.ContentPermutation.Attributes;

namespace QoDL.Toolkit.Dev.Common.ContentPermutation
{
    [TKContentPermutationType(MaxAllowedContentCount = 12, DefaultContentCount = 8)]
	public class ProductPermutations
	{
		public ProductType Type { get; set; }
		public bool IsExpired { get; set; }
		public bool IsOnSale { get; set; }

		public enum ProductType
		{
			Product = 0,
			Package,
			Bundle
		}
	}
}
