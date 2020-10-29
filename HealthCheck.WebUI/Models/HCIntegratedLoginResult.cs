namespace HealthCheck.WebUI.Models
{
    /// <summary>
    /// Result from custom login handler.
    /// </summary>
    public class HCIntegratedLoginResult
    {
        /// <summary>
        /// If true the dialog closes and the page refreshes.
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// If not successfull this message will be displayed.
        /// </summary>
        public string ErrorMessage { get; set; }
    }
}
