using HealthCheck.Core.Modules.ContentPermutation.Abstractions;
using HealthCheck.Core.Modules.ContentPermutation.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HealthCheck.Dev.Common.ContentPermutation
{
    public class DevOrderPermutationHandler : HCContentPermutationContentHandlerBase<OrderPermutations>
    {
		private readonly List<DevOrder> _all = new() {
			new DevOrder { Payment = OrderPermutations.PaymentType.Card, Details = "Card #1", Status = OrderPermutations.OrderStatus.Placed  },
			new DevOrder { Payment = OrderPermutations.PaymentType.Card, Details = "Card #2", Status = OrderPermutations.OrderStatus.Shipped  },
			new DevOrder { Payment = OrderPermutations.PaymentType.Invoice, Details = "Invoice #1", Status = OrderPermutations.OrderStatus.Placed  },
			new DevOrder { Payment = OrderPermutations.PaymentType.Invoice, Details = "Invoice #2", Status = OrderPermutations.OrderStatus.Packaged  },
			new DevOrder { Payment = OrderPermutations.PaymentType.Invoice, Details = "Invoice #3", Status = OrderPermutations.OrderStatus.Shipped  },
		};

		public override Task<List<HCPermutatedContentItemViewModel>> GetContentForAsync(OrderPermutations permutation)
        {
			var matches = _all.Where(x => permutation.Status == x.Status && permutation.PayType == x.Payment);
			var models = matches.Select(x => new HCPermutatedContentItemViewModel(x.Details, $"/somewhere")).ToList();
			return Task.FromResult(models);
		}

		public class DevOrder
		{
			public string Details { get; set; }
			public OrderPermutations.PaymentType Payment { get; set; }
			public OrderPermutations.OrderStatus Status { get; set; }
		}
	}
}
