using Newtonsoft.Json;
using QoDL.Toolkit.Core.Config;
using QoDL.Toolkit.WebUI.Exceptions;
using System;
using System.Collections.Generic;
using static QoDL.Toolkit.WebUI.Models.TKIntegratedLoginConfig;

namespace QoDL.Toolkit.WebUI.Models;

/// <summary>
/// Various front-end options for the web interface.
/// </summary>
public class TKFrontEndOptions
{
    /// <summary>
    /// Title of the page application.
    /// </summary>
    public string ApplicationTitle { get; set; } = "Health Check";

    /// <summary>
    /// Optional url the application title links to.
    /// </summary>
    public string ApplicationTitleLink { get; set; }

    /// <summary>
    /// Url to the endpoint.
    /// </summary>
    public string EndpointBase { get; set; }

    /// <summary>
    /// Url to the endpoint that invokes module methods.
    /// </summary>
    public string InvokeModuleMethodEndpoint { get; set; }

    /// <summary>
    /// Optional logout url title.
    /// <para>Defaults to "Logout"</para>
    /// </summary>
    public string LogoutLinkTitle { get; set; } = "Logout";

    /// <summary>
    /// Optional logout url.
    /// <para>Defaults to null, hiding it.</para>
    /// </summary>
    public string LogoutLinkUrl { get; set; }

    /// <summary>
    /// Include current query string in API calls to backend.
    /// <para>Enabled by default, can be used to set some roles based on query strings etc.</para>
    /// </summary>
    public bool InludeQueryStringInApiCalls { get; set; } = true;

    /// <summary>
    /// Config for the code editors.
    /// </summary>
    public EditorWorkerConfig EditorConfig { get; set; } = new EditorWorkerConfig();

    [JsonProperty]
    internal bool ShowIntegratedLogin { get; set; }

    [JsonProperty]
    internal string IntegratedLoginEndpoint { get; set; }

    [JsonProperty]
    internal DateTimeOffset? IntegratedLoginCurrent2FACodeExpirationTime { get; set; }

    [JsonProperty]
    internal int IntegratedLogin2FACodeLifetime { get; set; } = 30;

    [JsonProperty]
    internal string IntegratedLoginSend2FACodeEndpoint { get; set; }

    [JsonProperty]
    internal string IntegratedLogin2FANote { get; set; }

    [JsonProperty]
    internal string IntegratedLoginSend2FACodeButtonText { get; set; }

    [JsonProperty]
    internal string IntegratedLoginWebAuthnNote { get; set; }

    [JsonProperty]
    internal TKLoginWebAuthnMode IntegratedLoginWebAuthnMode { get; set; }

    [JsonProperty]
    internal TKLoginTwoFactorCodeInputMode IntegratedLoginTwoFactorCodeInputMode { get; set; }

    [JsonProperty]
    internal TKIntegratedProfileConfig IntegratedProfileConfig { get; set; }

    [JsonProperty]
    internal bool AllowAccessTokenKillswitch { get; set; }

    [JsonProperty]
    internal List<string> UserRoles { get; set; }

    [JsonProperty]
    internal List<TKUserModuleCategories> UserModuleCategories { get; set; } = new List<TKUserModuleCategories>();

    /// <summary>
    /// Create a new <see cref="TKFrontEndOptions"/>.
    /// </summary>
    /// <param name="baseApiEndpoint"></param>
    public TKFrontEndOptions(string baseApiEndpoint)
    {
        EndpointBase = baseApiEndpoint?.TrimEnd('/');
        InvokeModuleMethodEndpoint = $"{baseApiEndpoint?.TrimEnd('/')}/InvokeModuleMethod";
    }

    /// <summary>
    /// Validates values and throws <see cref="ConfigValidationException"/> if things are missing.
    /// </summary>
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(InvokeModuleMethodEndpoint))
            throw new ConfigValidationException($"{nameof(InvokeModuleMethodEndpoint)} is not set.");
    }

    /// <summary>
    /// Config for the monaco-editor.
    /// </summary>
    public class EditorWorkerConfig
    {
        /// <summary>
        /// Url to the editor worker script.
        /// <para>Can be prefixed with "blob:" to proxy it through a generated blob url.</para>
        /// <para>[base] can be used as a replacement for <c>EndpointBase</c></para>
        /// </summary>
        public string EditorWorkerUrl { get; set; }

        /// <summary>
        /// Url to the json worker script.
        /// <para>Can be prefixed with "blob:" to proxy it through a generated blob url.</para>
        /// <para>[base] can be used as a replacement for <c>EndpointBase</c></para>
        /// </summary>
        public string JsonWorkerUrl { get; set; }

        /// <summary>
        /// Url to the sql worker script.
        /// <para>Can be prefixed with "blob:" to proxy it through a generated blob url.</para>
        /// <para>[base] can be used as a replacement for <c>EndpointBase</c></para>
        /// </summary>
        public string SqlWorkerUrl { get; set; }

        /// <summary>
        /// Url to the sql worker script.
        /// <para>Can be prefixed with "blob:" to proxy it through a generated blob url.</para>
        /// <para>[base] can be used as a replacement for <c>EndpointBase</c></para>
        /// </summary>
        public string HtmlWorkerUrl { get; set; }

        internal void SetDefaults(string endpointBase)
        {
            EditorWorkerUrl ??= TKAssetGlobalConfig.DefaultEditorWorkerUrl ?? "";
            JsonWorkerUrl ??= TKAssetGlobalConfig.DefaultJsonWorkerUrl ?? "";
            SqlWorkerUrl ??= TKAssetGlobalConfig.DefaultSqlWorkerUrl ?? "";
            HtmlWorkerUrl ??= TKAssetGlobalConfig.DefaultHtmlWorkerUrl ?? "";

            EditorWorkerUrl = EditorWorkerUrl.Replace("[base]", endpointBase.TrimEnd('/'));
            JsonWorkerUrl = JsonWorkerUrl.Replace("[base]", endpointBase.TrimEnd('/'));
            SqlWorkerUrl = SqlWorkerUrl.Replace("[base]", endpointBase.TrimEnd('/'));
            HtmlWorkerUrl = HtmlWorkerUrl.Replace("[base]", endpointBase.TrimEnd('/'));
        }
    }

    internal class TKUserModuleCategories
    {
        public string ModuleId { get; set; }
        public string ModuleName { get; set; }
        public List<string> Categories { get; set; }
    }
}
