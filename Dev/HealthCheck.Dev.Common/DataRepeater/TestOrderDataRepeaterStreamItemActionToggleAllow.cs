using HealthCheck.Core.Attributes;
using HealthCheck.Core.Modules.DataRepeater.Abstractions;
using HealthCheck.Core.Modules.DataRepeater.Models;
using System.Threading.Tasks;

namespace HealthCheck.Dev.Common.DataRepeater
{
    public class TestOrderDataRepeaterStreamItemActionToggleAllow : HCDataRepeaterStreamItemActionBase<TestOrderDataRepeaterStreamItemActionToggleAllow.Parameters>
    {
        public override string DisplayName => "Set allow retry";

        public override string Description => "Sets AllowRetry property";

        public override string ExecuteButtonLabel => "Set";

        protected override Task<HCDataRepeaterStreamItemActionResult> PerformActionAsync(IHCDataRepeaterStream stream, IHCDataRepeaterStreamItem item, Parameters parameters)
        {
            var result = new HCDataRepeaterStreamItemActionResult
            {
                Success = true,
                AllowRetry = parameters.Allowed,
                Message = "Action A performed!"
            };
            return Task.FromResult(result);
        }

        public class Parameters
        {
            [HCCustomProperty]
            public bool Allowed { get; set; }
        }
    }
}
