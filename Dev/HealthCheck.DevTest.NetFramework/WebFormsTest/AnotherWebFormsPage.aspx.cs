using HealthCheck.Module.RequestLog.Util;
using HealthCheck.RequestLog.Services;
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