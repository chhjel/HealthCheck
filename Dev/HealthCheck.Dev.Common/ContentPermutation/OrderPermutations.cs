using HealthCheck.Core.Modules.ContentPermutation.Attributes;
using HealthCheck.Core.Modules.ContentPermutation.Models;

namespace HealthCheck.Dev.Common.ContentPermutation
{
    [HCContentPermutationType(Name = "Orders", Description = "Different variations of orders to test.")]
	public class OrderPermutations
	{
		public PaymentType PayType { get; set; }

		[HCContentPermutationProperty(DisplayName = "Order Status", Description = "Some description here.")]
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
