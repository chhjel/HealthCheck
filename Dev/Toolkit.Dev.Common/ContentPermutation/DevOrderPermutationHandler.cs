using QoDL.Toolkit.Core.Modules.ContentPermutation.Abstractions;
using QoDL.Toolkit.Core.Modules.ContentPermutation.Models;
using QoDL.Toolkit.Core.Util;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QoDL.Toolkit.Dev.Common.ContentPermutation;

public class DevOrderPermutationHandler : TKContentPermutationContentHandlerBase<OrderPermutations>
	{
    private static readonly List<DevOrder> _all = TKPermutationUtils.CreatePermutationsOf<OrderPermutations>()
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

		public override Task<List<TKPermutatedContentItemViewModel>> GetContentForAsync(TKGetContentPermutationContentOptions<OrderPermutations> options)
    {
			var permutation = options.Permutation;
			var matches = _all
				.Where(x => permutation.Status == x.Status && permutation.PayType == x.Payment)
				.Take(options.MaxCount);
			var models = matches
				.Select(x => new TKPermutatedContentItemViewModel(x.Details, $"/somewhere"))
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
