using QoDL.Toolkit.Core.Modules.DataRepeater.Abstractions;
using QoDL.Toolkit.Core.Util;
using System;

namespace QoDL.Toolkit.Core.Modules.DataRepeater.Models
{
    /// <summary>
    /// Result from <see cref="ITKDataRepeaterStreamItemAction.ExecuteActionAsync"/>
    /// </summary>
    public class TKDataRepeaterStreamItemActionResult : TKDataItemChangeBase
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
        public static TKDataRepeaterStreamItemActionResult CreateSuccess(string message = null) => new() { Success = true, Message = message };

        /// <summary></summary>
        public static TKDataRepeaterStreamItemActionResult CreateError(string error) => new() { Message = error };

        /// <summary></summary>
        public static TKDataRepeaterStreamItemActionResult CreateError(Exception ex) => new() { Message = TKExceptionUtils.GetFullExceptionDetails(ex) };
    }
}
