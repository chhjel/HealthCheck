using HealthCheck.Core.Modules.DataRepeater.Abstractions;
using HealthCheck.Core.Modules.DataRepeater.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HealthCheck.Dev.Common.DataRepeater
{
    public class TestOrderDataRepeaterStreamItemActionTest : HCDataRepeaterStreamItemActionBase<TestOrderDataRepeaterStreamItemActionTest.Parameters>
    {
        public override string DisplayName => "Action test with requirement";

        public override string Description => "Only allowed when processed";

        public override string ExecuteButtonLabel => "Test";

        public override Task<HCDataRepeaterStreamItemActionAllowedResult> ActionIsAllowedForAsync(IHCDataRepeaterStreamItem item)
        {
            var allowedOnItemsWithTags = new List<string> { "Processed", "Random tag", "Something" };
            if (!item.Tags.Any(t => allowedOnItemsWithTags.Contains(t)))
            {
                return Task.FromResult(HCDataRepeaterStreamItemActionAllowedResult.CreateNotAllowed("Missing required tag. Need one of " + string.Join(", ", allowedOnItemsWithTags)));
            }
            return Task.FromResult(HCDataRepeaterStreamItemActionAllowedResult.CreateAllowed());
        }

        protected override Task<HCDataRepeaterStreamItemActionResult> PerformActionAsync(IHCDataRepeaterStreamItem item, Parameters parameters)
        {
            var result = new HCDataRepeaterStreamItemActionResult
            {
                Success = true,
                Message = "Ok!",
                RemoveAllTags = true
            };
            return Task.FromResult(result);
        }

        public class Parameters
        {
        }
    }
}
