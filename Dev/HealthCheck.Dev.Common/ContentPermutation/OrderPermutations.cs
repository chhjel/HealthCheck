using HealthCheck.Core.Modules.ContentPermutation.Attributes;
using HealthCheck.Core.Modules.ContentPermutation.Models;

namespace HealthCheck.Dev.Common.ContentPermutation
{
    [HCContentPermutationType]
	public class OrderPermutations
	{
		public PaymentType PayType { get; set; }
		public OrderStatus Status { get; set; }

		public enum PaymentType
		{
			Invoice = 0,
			Card
		}
		public enum OrderStatus
		{
			Placed = 0,
			Packaged,
			Shipped
		}
	}
}
