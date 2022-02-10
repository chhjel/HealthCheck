using HealthCheck.Core.Util.Models;
using System;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading.Tasks;

namespace HealthCheck.Core.Util
{
    /// <summary>
    /// Some connectivity related utility methods.
    /// </summary>
    public static class HCConnectivityUtils
    {
        /// <summary>
        /// Sends an Internet Control Message Protocol (ICMP) echo message to the specified computer.
        /// </summary>
        /// <param name="hostname">Hostname to ping</param>
        /// <param name="timeoutMilliseconds">Timeout of the ping</param>
        public static async Task<HCConnectivityCheckResult> PingHost(string hostname, int timeoutMilliseconds = 5000)
        {
            var action = $"Ping '{hostname}'";

            try
            {
                var ping = new Ping();
                var result = await ping.SendPingAsync(hostname, timeoutMilliseconds);
                if (result.Status == IPStatus.Success)
                {
                    return HCConnectivityCheckResult.CreateSuccess(action, $"Roundtrip time was {result.RoundtripTime}ms.");
                } else
                {
                    return HCConnectivityCheckResult.CreateError(action, $"Failed with a status of '{result.Status}'.");
                }
            }
            catch (Exception ex)
            {
                return HCConnectivityCheckResult.CreateError(action, ex);
            }
        }

        /// <summary>
        /// Perform the given web request against the given url.
        /// </summary>
        /// <param name="url">Url to perform a request against.</param>
        /// <param name="method">Request method.</param>
        /// <param name="timeoutMs">Request timout duration.</param>
        /// <param name="allowedStatusCodes">If the response status code is one of the given values it will not fail. Defaults to 200.</param>
        public static async Task<HCConnectivityCheckResult> PerformWebRequestCheck(
            string url, string method = "HEAD",
            int timeoutMs = 5000,
            int[] allowedStatusCodes = null)
        {
            allowedStatusCodes ??= new[] { 200 };
            var action = $"'{method}'-request to '{url}'";

            try
            {
                var request = HttpWebRequest.Create(url) as HttpWebRequest;
                request.Timeout = timeoutMs;
                request.Method = method;

                using HttpWebResponse response = (await request.GetResponseAsync()) as HttpWebResponse;
                int statusCode = (int)response.StatusCode;
                if (allowedStatusCodes.Any(x => statusCode == x))
                {
                    return HCConnectivityCheckResult.CreateSuccess(action, $"Statuscode '{statusCode}' was returned.");
                }
                else
                {
                    return HCConnectivityCheckResult.CreateError(action, $"Statuscode '{statusCode}' was returned.");
                }
            }
            catch (WebException wex) when (wex.Response is HttpWebResponse webResponse
                && webResponse?.StatusCode != null
                && allowedStatusCodes.Any(x => (int)webResponse.StatusCode == x))
            {
                int statusCode = (int)webResponse.StatusCode;
                return HCConnectivityCheckResult.CreateSuccess(action, $"Statuscode '{statusCode}' was returned.");
            }
            catch(Exception ex)
            {
                return HCConnectivityCheckResult.CreateError(action, ex);
            }
        }
    }
}
