using HealthCheck.RequestLog.Services;
using HealthCheck.RequestLog.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

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