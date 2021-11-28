using HealthCheck.Core.Modules.DataRepeater.Abstractions;
using HealthCheck.Core.Modules.DataRepeater.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HealthCheck.Dev.Common.DataRepeater
{
    public class TestOrderDataRepeaterStreamItemActionTest : HCDataRepeaterStreamItemActionBase<TestOrderDataRepeaterStreamItemActionTest.Parameters>
    {
        public override List<string> AllowedOnItemsWithTags => new List<string> { "Processed", "Random tag", "Something" };

        public override string DisplayName => "Action test with requirement";

        public override string Description => "Only allowed when processed";

        public override string ExecuteButtonLabel => "Test";

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
