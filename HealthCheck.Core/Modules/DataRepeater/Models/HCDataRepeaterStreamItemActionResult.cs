using HealthCheck.Core.Modules.DataRepeater.Abstractions;
using HealthCheck.Core.Util;
using System;

namespace HealthCheck.Core.Modules.DataRepeater.Models
{
    /// <summary>
    /// Result from <see cref="IHCDataRepeaterStreamItemAction.ExecuteActionAsync"/>
    /// </summary>
    public class HCDataRepeaterStreamItemActionResult : HCDataItemChangeBase
    {
        /// <summary></summary>
        public bool Success { get; set; }

        /// <summary></summary>
        public bool HasError => !Success;

        /// <summary>
        /// Status message that will be logged for either error or success.
        /// <para>If null or empty a default one will be created.</para>
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Optionally delete the item.
        /// </summary>
        public bool Delete { get; set; }

        /// <summary></summary>
        public static HCDataRepeaterStreamItemActionResult CreateSuccess(string message = null) => new() { Success = true, Message = message };

        /// <summary></summary>
        public static HCDataRepeaterStreamItemActionResult CreateError(string error) => new() { Message = error };

        /// <summary></summary>
        public static HCDataRepeaterStreamItemActionResult CreateError(Exception ex) => new() { Message = HCExceptionUtils.GetFullExceptionDetails(ex) };
    }
}
