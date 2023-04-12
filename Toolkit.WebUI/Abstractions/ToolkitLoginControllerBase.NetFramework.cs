#if NETFULL
using QoDL.Toolkit.Core.Attributes;
using QoDL.Toolkit.WebUI.Models;
using QoDL.Toolkit.WebUI.Util;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace QoDL.Toolkit.WebUI.Abstractions;

/// <summary>
/// Base controller for integrated toolkit login.
/// </summary>
public abstract class ToolkitLoginControllerBase : Controller
{
    /// <summary>
    /// Set to false to return 404 for all actions.
    /// <para>Enabled by default.</para>
    /// </summary>
    protected bool Enabled { get; set; } = true;

    /// <summary>
    /// Minimum added delay on login to limit brute force.
    /// </summary>
    protected TimeSpan? MinAddedDelay { get; set; } = TimeSpan.FromSeconds(1.5);

    /// <summary>
    /// Maximum added delay on login to limit brute force.
    /// </summary>
    protected TimeSpan? MaxAddedDelay { get; set; } = TimeSpan.FromSeconds(3);

    #region Endpoints
    /// <summary>
    /// Attempts to login using custom logic from <see cref="HandleLoginRequest"/>.
    /// </summary>
    [HideFromRequestLog]
    [HttpPost]
    public virtual async Task<ActionResult> Login(TKIntegratedLoginRequest model)
    {
        if (!Enabled) return HttpNotFound();
        await Delay();

        var result = HandleLoginRequest(model);
        if (result == null) return HttpNotFound();

        return CreateJsonResult(result);
    }

    /// <summary>
    /// Invoked when requesting a 2FA code.
    /// </summary>
    [HideFromRequestLog]
    [HttpPost]
    public virtual async Task<ActionResult> Request2FACode(TKIntegratedLoginRequest2FACodeRequest model)
    {
        if (!Enabled) return HttpNotFound();
        await Delay();

        var result = Handle2FACodeRequest(model);
        if (result == null) return HttpNotFound();

        return CreateJsonResult(result);
    }

    /// <summary>
    /// Handles WebAuthn assertion options creation.
    /// </summary>
    [HideFromRequestLog]
    [HttpPost]
    public virtual ActionResult CreateWebAuthnAssertionOptions(TKIntegratedLoginCreateWebAuthnAssertionOptionsRequest request)
    {
        if (!Enabled) return HttpNotFound();
        else if (!ModelState.IsValid)
        {
            var errors = string.Join("\n", ModelState.SelectMany(x => x.Value.Errors).Select(x => x.ErrorMessage));
            return new HttpStatusCodeResult(HttpStatusCode.BadRequest, errors);
        }

        var optionsJson = CreateWebAuthnAssertionOptionsJson(request);
        if (optionsJson == null)
        {
            return Json(new { status = "error", error = "User not found." });
        }

        Session["WebAuthn.assertionOptions"] = optionsJson;
        return Content(optionsJson, "application/json");
    }
    #endregion

    #region Overridables
    /// <summary>
    /// Handle login request here.
    /// </summary>
    protected abstract TKIntegratedLoginResult HandleLoginRequest(TKIntegratedLoginRequest request);

    /// <summary>
    /// Optionally handle 2FA code request here.
    /// </summary>
    protected virtual TKIntegratedLogin2FACodeRequestResult Handle2FACodeRequest(TKIntegratedLoginRequest2FACodeRequest request) => null;

    /// <summary>
    /// Handles WebAuthn assertion options creation.
    /// </summary>
    protected virtual string CreateWebAuthnAssertionOptionsJson(TKIntegratedLoginCreateWebAuthnAssertionOptionsRequest request) => null;

    /// <summary>
    /// Retrieves 'WebAuthn.assertionOptions' from session.
    /// </summary>
    protected virtual string GetWebAuthnAssertionOptionsJsonForSession() => Session["WebAuthn.assertionOptions"] as string;
    #endregion

    #region Helpers
    /// <summary>
    /// Serializes the given object into a json result.
    /// </summary>
    protected ActionResult CreateJsonResult(object obj, bool stringEnums = true)
        => Content(ToolkitLoginControllerHelper.SerializeJson(obj, stringEnums), "application/json");

    /// <summary>
    /// Delay by the configured amount.
    /// </summary>
    protected async Task Delay()
    {
        if (MinAddedDelay != null && MaxAddedDelay != null)
        {
            var random = new Random();
            var msDelay = random.Next((int)MinAddedDelay.Value.TotalMilliseconds, (int)MaxAddedDelay.Value.TotalMilliseconds);
            await Task.Delay(msDelay);
        }
    }

    /// <summary>
    /// Creates a new code for the given and sets it in session.
    /// </summary>
    protected string CreateSession2FACode(string username, int length = 6)
    {
        var code = ToolkitLoginControllerHelper.Generate2FACode(length);
        Session[CreateSession2FACodeKey(username)] = code;
        return code;
    }

    /// <summary>
    /// Validates the given code against the last generated code for the given user.
    /// <para>If the code is valid it will be cleared from session.</para>
    /// </summary>
    protected bool ValidateSession2FACode(string username, string code)
    {
        var key = CreateSession2FACodeKey(username);
        var valid = Session[key] as string == code;
        if (valid)
        {
            Session.Remove(key);
        }
        return valid;
    }

    private string CreateSession2FACodeKey(string username)
        => $"__tk_Session2FACode_{username?.ToLower()}";
    #endregion
}
#endif
