using System;

namespace HealthCheck.WebUI.Models
{
    /// <summary>
    /// Result from custom login handler.
    /// </summary>
    public class HCIntegratedLogin2FACodeRequestResult
    {
        /// <summary>
        /// If true the dialog closes and the page refreshes.
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// If successfull this message will be displayed.
        /// </summary>
        public string SuccessMessage { get; set; }

        /// <summary>
        /// Allow rendering success message as html. Defaults to false.
        /// </summary>
        public bool ShowSuccessAsHtml { get; set; }

        /// <summary>
        /// If not successfull this message will be displayed.
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// Allow rendering error message as html. Defaults to false.
        /// </summary>
        public bool ShowErrorAsHtml { get; set; }

        /// <summary>
        /// Can be set from <see cref="CreateSuccess"/> when <c>codeExpiresIn</c> is set.
        /// <para>Causes a progress bar to be shown.</para>
        /// </summary>
        public int? CodeExpiresInSeconds { get; internal set; }

        /// <summary>
        /// Create a new error result.
        /// </summary>
        /// <param name="error">The error message to display.</param>
        /// <param name="showAsHtml">Allow rendering error message as html. Defaults to false.</param>
        public static HCIntegratedLogin2FACodeRequestResult CreateError(string error, bool showAsHtml = false)
            => new()
            { Success = false, ErrorMessage = error, ShowErrorAsHtml = showAsHtml };

        /// <summary>
        /// Create a new result, success or error depending on the given bool.
        /// </summary>
        /// <param name="success">If true, login is allowed.</param>
        /// <param name="error">If not successfull this message will be displayed.</param>
        /// <param name="showAsHtml">Allow rendering error message as html. Defaults to false.</param>
        public static HCIntegratedLogin2FACodeRequestResult CreateResult(bool success, string error, bool showAsHtml = false)
            => new()
            {
                Success = success,
                ErrorMessage = !success ? error : "",
                ShowErrorAsHtml = showAsHtml
            };

        /// <summary>
        /// Create a new result allowing login.
        /// </summary>
        public static HCIntegratedLogin2FACodeRequestResult CreateSuccess(string message = null, bool showAsHtml = false, TimeSpan? codeExpiresIn = null)
            => new()
            {
                Success = true,
                SuccessMessage = message,
                ShowSuccessAsHtml = showAsHtml,
                CodeExpiresInSeconds = codeExpiresIn.HasValue ? (int)codeExpiresIn.Value.TotalSeconds : default
            };
    }
}
