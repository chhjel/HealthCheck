using HealthCheck.Core.Modules.DataRepeater.Abstractions;
using HealthCheck.Core.Modules.DataRepeater.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HealthCheck.Dev.Common.DataRepeater
{
    public class TestOrderDataRepeaterStreamItemActionRemoveAllTags : HCDataRepeaterStreamItemActionBase<TestOrderDataRepeaterStreamItemActionRemoveAllTags.Parameters>
    {
        public override string DisplayName => "Remove all tags";

        public override string Description => "Removes all tags from the item.";

        public override string ExecuteButtonLabel => "Remove tags";
        
        public override object AllowedAccessRoles => RuntimeTestAccessRole.SystemAdmins;

        public override List<string> Categories => new() { "Secret actions" };

        protected override Task<HCDataRepeaterStreamItemActionResult> PerformActionAsync(IHCDataRepeaterStream stream, IHCDataRepeaterStreamItem item, Parameters parameters)
        {
            var result = new HCDataRepeaterStreamItemActionResult
            {
                Success = true,
                Message = "Tags removed!",
                RemoveAllTags = true
            };
            return Task.FromResult(result);
        }

        public class Parameters
        {
        }
    }
}
