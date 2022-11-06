namespace HealthCheck.Core.Modules.Jobs.Models
{
    /// <summary></summary>
    public class HCJobSimpleResult
    {
        /// <summary></summary>
        public bool Success { get; set; }
        /// <summary></summary>
        public string Message { get; set; }

        /// <summary></summary>
        public static HCJobSimpleResult CreateSuccess(string message = null)
            => new() { Success = true, Message = message };

        /// <summary></summary>
        public static HCJobSimpleResult CreateError(string message)
            => new() { Message = message };
    }
}
