using HealthCheck.Core.Modules.DataRepeater.Abstractions;
using HealthCheck.Core.Modules.DataRepeater.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HealthCheck.Core.Modules.DataRepeater.Services
{
    /// <summary>
    /// Handles repeater streams for reprocessing data.
    /// </summary>
    public class HCDataRepeaterService : IHCDataRepeaterService
    {
        /// <inheritdoc />
        public Task<IEnumerable<IHCDataRepeaterStream>> GetStreamsAsync()
        {
            throw new NotImplementedException();
        }

        //public virtual async Task AnalyseAll ?

        /// <summary>
        /// Attempts to retry an item.
        /// </summary>
        public virtual async Task<HCDataRepeaterActionResult> RetryItemAsync(string streamTypeName, IHCDataRepeaterStreamItem item)
        {
            var stream = (await GetStreamsAsync())
                .FirstOrDefault(x => x.GetType().FullName == streamTypeName);
            var result = await stream.RetryItemAsync(item);

            var shouldSave = false;
            if (result?.AllowRetry != null && item.AllowRetry != result.AllowRetry)
            {
                item.AllowRetry = result.AllowRetry.Value;
                shouldSave = true;
            }

            if (result.RemoveAllTags && item.Tags?.Any() == true)
            {
                item.Tags.Clear();
                shouldSave = true;
            }

            if (result?.TagsThatShouldExist?.Any() == true)
            {
                foreach (var tag in result.TagsThatShouldExist)
                {
                    item.Tags ??= new HashSet<string>();
                    if (!item.Tags.Contains(tag))
                    {
                        item.Tags.Add(tag);
                        shouldSave = true;
                    }
                }
            }

            if (result?.TagsThatShouldNotExist?.Any() == true)
            {
                foreach (var tag in result.TagsThatShouldNotExist)
                {
                    item.Tags ??= new HashSet<string>();
                    if (item.Tags.Contains(tag))
                    {
                        item.Tags.Remove(tag);
                        shouldSave = true;
                    }
                }
            }

            if (shouldSave)
            {
                await stream.UpdateItemAsync(item);
            }

            return result;
        }
    }
}
