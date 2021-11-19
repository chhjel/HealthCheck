using HealthCheck.Core.Modules.DataRepeater.Abstractions;
using HealthCheck.Core.Modules.DataRepeater.Models;
using System.Collections.Generic;
using System.Linq;

namespace HealthCheck.Core.Modules.DataRepeater.Utils
{
    /// <summary>
    /// Utilities related to the data repeater module.
    /// </summary>
    public static class HCDataRepeaterUtils
    {
        /// <summary>
        /// Applies changes to the given item.
        /// </summary>
        public static void ApplyChangesToItem(IHCDataRepeaterStreamItem item, HCDataItemChangeBase changes)
        {
            if (changes == null || item == null) return;

            if (changes?.AllowRetry != null)
            {
                item.AllowRetry = changes.AllowRetry.Value;
            }

            item.Tags ??= new HashSet<string>();
            if (changes.RemoveAllTags && item.Tags?.Any() == true)
            {
                item.Tags.Clear();
            }

            if (changes?.TagsThatShouldNotExist?.Any() == true)
            {
                foreach (var tag in changes.TagsThatShouldNotExist)
                {
                    item.Tags.Remove(tag);
                }
            }

            if (changes?.TagsThatShouldExist?.Any() == true)
            {
                foreach (var tag in changes.TagsThatShouldExist)
                {
                    item.Tags.Add(tag);
                }
            }
        }
    }
}
