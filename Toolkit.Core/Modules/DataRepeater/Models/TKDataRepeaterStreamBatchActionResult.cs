using QoDL.Toolkit.Core.Modules.DataRepeater.Abstractions;
using System.Collections.Generic;
using System.Text;

namespace QoDL.Toolkit.Core.Modules.DataRepeater.Models
{
    /// <summary>
    /// Result from <see cref="ITKDataRepeaterStreamItemAction.ExecuteActionAsync"/>
    /// </summary>
    public class TKDataRepeaterStreamBatchActionResult
    {
        /// <summary></summary>
        public bool Success => UpdateFailedCount == 0 && string.IsNullOrWhiteSpace(ErrorMessage);

        /// <summary>
        /// Status message that will be logged for errors.
        /// </summary>
        public string ErrorMessage { get; internal set; }

        /// <summary></summary>
        public string Message => ToString();

        /// <summary></summary>
        public int UpdatedSuccessfullyCount { get; internal set; }

        /// <summary></summary>
        public int NotAttemptedUpdatedCount { get; internal set; }

        /// <summary></summary>
        public int UpdateFailedCount { get; internal set; }

        /// <summary></summary>
        public int AttemptedUpdatedCount => UpdatedSuccessfullyCount + UpdateFailedCount;

        /// <summary></summary>
        public static TKDataRepeaterStreamBatchActionResult CreateError(string error) => new() { ErrorMessage = error };

        /// <summary></summary>
        public override string ToString()
        {
            var builder = new StringBuilder();

            var parts = new List<string>();
            if (UpdatedSuccessfullyCount > 0)
            {
                parts.Add($"Updated successfully: {UpdatedSuccessfullyCount}");
            }
            if (UpdateFailedCount > 0)
            {
                parts.Add($"Update failed: {UpdateFailedCount}");
            }
            if (NotAttemptedUpdatedCount > 0)
            {
                parts.Add($"Not updated: {NotAttemptedUpdatedCount}");
            }

            if (parts.Count > 0)
            {
                builder.Append(string.Join(", ", parts));
            }
            else
            {
                builder.Append("Nothing was done.");
            }

            if (!string.IsNullOrWhiteSpace(ErrorMessage))
            {
                builder.Append($" {ErrorMessage}");
            }
            return builder.ToString();
        }
    }
}
