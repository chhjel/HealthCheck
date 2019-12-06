namespace HealthCheck.Core.Modules.RequestLog.Enums
{
    /// <summary>
    /// 
    /// </summary>
    public enum LogFilterMethod
    {
        /// <summary>
        /// After MVC action was executed.
        /// </summary>
        OnActionExecuted = 0,

        /// <summary>
        /// After WebAPI action was executed.
        /// </summary>
        OnResultExecuted,

        /// <summary>
        /// On unhandled exception.
        /// </summary>
        OnException
    }
}
