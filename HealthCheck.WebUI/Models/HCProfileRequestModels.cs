namespace HealthCheck.WebUI.Models
{
    ///<summary></summary>
    public class HCProfileRegisterTotpRequest
    {
        ///<summary></summary>
        public string Secret { get; set; }

        ///<summary></summary>
        public string Code { get; set; }

        ///<summary></summary>
        public string Password { get; set; }
    }

    ///<summary></summary>
    public class HCProfileRemoveTotpRequest
    {
        ///<summary></summary>
        public string Password { get; set; }
    }

    ///<summary></summary>
    public class HCProfileElevateTotpRequest
    {
        ///<summary></summary>
        public string Code { get; set; }
    }

    ///<summary></summary>
    public class HCProfileRemoveWebAuthnRequest
    {
        ///<summary></summary>
        public string Password { get; set; }
    }

    ///<summary></summary>
    public class HCProfileElevateWebAuthnRequest
    {
        ///<summary></summary>
        public HCVerifyWebAuthnAssertionModel Data { get; set; }
    }

    ///<summary></summary>
    public class HCProfileRegisterWebAuthnRequest
    {
        ///<summary></summary>
        public string Password { get; set; }

        ///<summary></summary>
        public string RegistrationData { get; set; }
    }

    ///<summary></summary>
    public class HCCreateWebAuthnRegistrationOptionsRequest
    {
        ///<summary></summary>
        public string UserName { get; set; }

        ///<summary></summary>
        public string Password { get; set; }
    }
}
