using QoDL.Toolkit.Core.Attributes;
using QoDL.Toolkit.Core.Modules.ContentPermutation.Attributes;
using QoDL.Toolkit.Core.Modules.ContentPermutation.Models;

namespace QoDL.Toolkit.Dev.Common.ContentPermutation;

[TKContentPermutationType(Name = "Orders", Description = "Different variations of orders to test.")]
	public class OrderPermutations
	{
		public PaymentType PayType { get; set; }

    [TKCustomProperty(Name = "Order Status", Description = "Some description here.")]
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
