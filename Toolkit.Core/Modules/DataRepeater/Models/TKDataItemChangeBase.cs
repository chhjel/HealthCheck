using QoDL.Toolkit.Core.Modules.DataRepeater.Abstractions;
using System;
using System.Collections.Generic;

namespace QoDL.Toolkit.Core.Modules.DataRepeater.Models
{
    /// <summary></summary>
    public class TKDataItemChangeBase
    {
        /// <summary>
        /// Optionally set if item can be retried or not.
        /// </summary>
        public bool? AllowRetry { get; set; }

        /// <summary>
        /// Optionally remove all item tags.
        /// <para><see cref="TagsThatShouldExist"/> will still take effect.</para>
        /// </summary>
        public bool RemoveAllTags { get; set; }

        /// <summary>
        /// Tags that will be applied if missing.
        /// </summary>
        public List<string> TagsThatShouldExist { get; set; } = new();

        /// <summary>
        /// Tags that will be removed if present.
        /// </summary>
        public List<string> TagsThatShouldNotExist { get; set; } = new();

        /// <summary>
        /// Any modifications to the stream item.
        /// </summary>
        internal Action<object> StreamItemModification { get; set; }

        /// <summary>
        /// Perform any modifications to the stream item.
        /// </summary>
        public TKDataItemChangeBase SetStreamItemModification<TStreamItem>(Action<TStreamItem> modification)
            where TStreamItem : class, ITKDataRepeaterStreamItem
        {
            if (modification == null)
            {
                StreamItemModification = null;
            }
            else
            {
                StreamItemModification = x => modification?.Invoke(x as TStreamItem);
            }
            return this;
        }
    }
}
