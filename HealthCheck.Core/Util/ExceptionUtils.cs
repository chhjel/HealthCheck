using HealthCheck.Core.Extensions;
using System;
using System.IO;
using System.Net;

namespace HealthCheck.Core.Util
{
    /// <summary>
    /// Exception related utility methods.
    /// </summary>
    public static class ExceptionUtils
    {
        /// <summary>
        /// Gets a full summary of the exception. Including inner exception message, stack trace and potentially other usefull data.
        /// </summary>
        public static string GetFullExceptionDetails(Exception exception)
        {
            if (exception == null)
            {
                return $"Exception is null.";
            }

            var summary = GetExceptionSummary(exception);
            var details = GetExceptionDetails(exception);
            var trace = exception?.StackTrace;

            var result = summary;

            if (!string.IsNullOrWhiteSpace(details))
            {
                result += $"\n\nDetails:\n{details}";
            }

            if (!string.IsNullOrWhiteSpace(trace))
            {
                result += $"\n\nStack trace:\n{trace}";
            }

            return result;
        }

        /// <summary>
        /// Gets a summary of the exception. If it's a WebException the full response will be attempted read and included.
        /// </summary>
        public static string GetExceptionSummary(Exception exception)
        {
            if (exception == null)
            {
                return $"Exception is null.";
            }

            var exceptionMessage = exception?.Message?.EnsureEndsWithIfNotNull(".");
            var innerExceptionMessage = exception?.InnerException?.Message?.EnsureEndsWithIfNotNull(".");
            if (!string.IsNullOrWhiteSpace(innerExceptionMessage))
            {
                innerExceptionMessage = $" {innerExceptionMessage}";
            }

            if (exception is WebException wex)
            {
                if (wex.Status == WebExceptionStatus.NameResolutionFailure)
                {
                    return $"NameResolutionFailure, could not resolve service DNS name. {exceptionMessage}{innerExceptionMessage}";
                }
                else if (wex.Status == WebExceptionStatus.ProtocolError)
                {
                    string output = $"{exceptionMessage}{innerExceptionMessage}";
                    if (wex.Response is HttpWebResponse webResponse)
                    {
                        var statusCode = (int)webResponse.StatusCode;
                        output += $" Status code: {statusCode} ({webResponse.StatusCode}).";
                    }
                    return output;
                }
                else
                {
                    return $"{exceptionMessage} Status: {wex.Status}.{innerExceptionMessage}";
                }
            }
            else
            {
                return $"{exception?.Message} (Exception type: {exception?.GetType()?.Name}).{innerExceptionMessage}";
            }
        }

        /// <summary>
        /// Try to get some details of the given exception. If it's a WebException the full response will be attempted read and included.
        /// </summary>
        public static string GetExceptionDetails(Exception exception)
        {
            var result = "";
            if (exception is WebException wex)
            {
                result = TryReadWebExceptionResponse(wex);
            }
            return string.IsNullOrWhiteSpace(result) ? null : result;
        }

        /// <summary>
        /// Returns the web exception response stream as a string, or null if not possible.
        /// </summary>
        public static string TryReadWebExceptionResponse(WebException wex)
        {
            if (wex == null || !(wex.Response is HttpWebResponse httpWebResponse))
            {
                return null;
            }

            try
            {
                return new StreamReader(httpWebResponse.GetResponseStream()).ReadToEnd();
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
