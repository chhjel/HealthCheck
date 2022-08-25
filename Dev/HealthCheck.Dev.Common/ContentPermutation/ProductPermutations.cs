﻿using HealthCheck.Core.Modules.ContentPermutation.Attributes;

namespace HealthCheck.Dev.Common.ContentPermutation
{
    [HCContentPermutationType]
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