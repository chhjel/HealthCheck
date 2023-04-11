namespace QoDL.Toolkit.WebUI.Models;

///<summary></summary>
public class TKProfileRegisterTotpRequest
{
    ///<summary></summary>
    public string Secret { get; set; }

    ///<summary></summary>
    public string Code { get; set; }

    ///<summary></summary>
    public string Password { get; set; }
}

///<summary></summary>
public class TKProfileRemoveTotpRequest
{
    ///<summary></summary>
    public string Password { get; set; }
}

///<summary></summary>
public class TKProfileElevateTotpRequest
{
    ///<summary></summary>
    public string Code { get; set; }
}

///<summary></summary>
public class TKProfileRemoveWebAuthnRequest
{
    ///<summary></summary>
    public string Password { get; set; }
}

///<summary></summary>
public class TKProfileElevateWebAuthnRequest
{
    ///<summary></summary>
    public TKVerifyWebAuthnAssertionModel Data { get; set; }
}

///<summary></summary>
public class TKProfileRegisterWebAuthnRequest
{
    ///<summary></summary>
    public string Password { get; set; }

    ///<summary></summary>
    public TKRegisterWebAuthnModel RegistrationData { get; set; }
}

///<summary></summary>
public class TKCreateWebAuthnRegistrationOptionsRequest
{
    ///<summary></summary>
    public string UserName { get; set; }

    ///<summary></summary>
    public string Password { get; set; }
}

///<summary></summary>
public class TKCreateWebAuthnAssertionOptionsRequest
{
    ///<summary></summary>
    public string UserName { get; set; }
}
