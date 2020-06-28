using HealthCheck.RequestLog.Services;
using HealthCheck.RequestLog.Util;
using System;

namespace HealthCheck.DevTest.WebFormsTest
{
    public partial class AnotherWebFormsPage : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            RequestLogUtils.HandleRequest(RequestLogServiceAccessor.Current, GetType().BaseType ?? GetType(), Request, forcedControllerType: "WebForms");
        }
    }
}