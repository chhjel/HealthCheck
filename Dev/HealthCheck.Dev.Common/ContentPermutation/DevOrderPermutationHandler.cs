using HealthCheck.Core.Modules.ContentPermutation.Abstractions;
using HealthCheck.Core.Modules.ContentPermutation.Models;
using HealthCheck.Core.Util;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HealthCheck.Dev.Common.ContentPermutation
{
    public class DevOrderPermutationHandler : HCContentPermutationContentHandlerBase<OrderPermutations>
	{
        private static readonly List<DevOrder> _all = HCPermutationUtils.CreatePermutationsOf<OrderPermutations>()
			.SelectMany((p, i) => Enumerable.Range(0, 10)
				.Select(e => new DevOrder
				{
					OrderNumber = ((i * 10) + e) + 88000,
					Details = $"{p.PayType} #{((i * 10) + e) + 88000} ({p.Status})",
					Payment = p.PayType,
					Status = p.Status
				})
			)
			.ToList();

		public override Task<List<HCPermutatedContentItemViewModel>> GetContentForAsync(HCGetContentPermutationContentOptions<OrderPermutations> options)
        {
			var permutation = options.Permutation;
			var matches = _all
				.Where(x => permutation.Status == x.Status && permutation.PayType == x.Payment)
				.Take(options.MaxCount);
			var models = matches
				.Select(x => new HCPermutatedContentItemViewModel(x.Details, $"/somewhere"))
				.ToList();
			return Task.FromResult(models);
		}

		public class DevOrder
		{
			public int OrderNumber { get; set; }
			public string Details { get; set; }
			public OrderPermutations.PaymentType Payment { get; set; }
			public OrderPermutations.OrderStatus Status { get; set; }
		}
	}
}
