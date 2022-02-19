using System.Net;

namespace HealthCheck.Core.Util.Modules
{
    /// <summary>
    /// Can be returned from module actions to return a status code with empty reslt.
    /// </summary>
    public class HealthCheckStatusCodeOnlyResult
	{
        /// <summary>
        /// Status code to return.
        /// </summary>
        public HttpStatusCode Code { get; set; }

        /// <summary>
        /// Can be returned from module actions to return a status code with empty reslt.
        /// </summary>
        public HealthCheckStatusCodeOnlyResult(HttpStatusCode code)
        {
            Code = code;
        }
    }
}
