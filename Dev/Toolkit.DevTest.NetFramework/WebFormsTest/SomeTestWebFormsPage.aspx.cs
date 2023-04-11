using QoDL.Toolkit.Module.RequestLog.Util;
using QoDL.Toolkit.RequestLog.Services;
using System;

namespace QoDL.Toolkit.DevTest.WebFormsTest
{
    public partial class SomeTestWebFormsPage : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            RequestLogUtils.HandleRequest(RequestLogServiceAccessor.Current, GetType().BaseType ?? GetType(), Request, forcedControllerType: "WebForms");
        }
    }
}
