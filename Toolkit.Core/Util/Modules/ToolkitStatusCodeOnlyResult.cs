using System.Net;

namespace QoDL.Toolkit.Core.Util.Modules
{
    /// <summary>
    /// Can be returned from module actions to return a status code with empty reslt.
    /// </summary>
    public class ToolkitStatusCodeOnlyResult
	{
        /// <summary>
        /// Status code to return.
        /// </summary>
        public HttpStatusCode Code { get; set; }

        /// <summary>
        /// Can be returned from module actions to return a status code with empty reslt.
        /// </summary>
        public ToolkitStatusCodeOnlyResult(HttpStatusCode code)
        {
            Code = code;
        }
    }
}
