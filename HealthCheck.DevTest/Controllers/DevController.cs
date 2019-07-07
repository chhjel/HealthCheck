using System.IO;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;
using HealthCheck.Core.TestManagers;
using HealthCheck.Core.Util;
using HealthCheck.DevTest._TestImplementation;
using HealthCheck.WebUI.Models;
using HealthCheck.WebUI.ViewModels;
using HealthCheck.WebUI.Abstractions;
using HealthCheck.WebUI.Util;

namespace HealthCheck.DevTest.Controllers
{
    public class DevController : HealthCheckControllerBase<RuntimeTestAccessRole>
    {
        private const string EndpointBase = "/dev";

        public DevController()
            : base(assemblyContainingTests: typeof(DevController).Assembly) {}


        #region Overrides
        protected override FrontEndOptionsViewModel GetFrontEndOptions()
            => new FrontEndOptionsViewModel(EndpointBase)
            {
                ApplicationTitle = "Test Monitor"
            };

        protected override PageOptions GetPageOptions()
            => new PageOptions()
            {
                JavaScriptUrl = $"{EndpointBase}/GetScript",
                PageTitle = "Test Monitor"
            };

        protected override void SetOptionalOptions(HttpRequestBase request, TestRunner testRunner, TestDiscoverer testDiscoverer)
        {
            var requestRoles = GetRequestAccessRoles(request);
            testRunner.IncludeExceptionStackTraces = requestRoles.HasValue && requestRoles.Value.HasFlag(RuntimeTestAccessRole.SystemAdmins);
            testDiscoverer.GroupOptions
                .SetOptionsFor(RuntimeTestConstants.Group.AdminStuff, uiOrder: 100, iconName: RuntimeTestConstants.Icons.Face)
                .SetOptionsFor(RuntimeTestConstants.Group.BottomGroup, uiOrder: -100, iconName: WebIcons.Grouped.File.Cloud);
        }

        protected override Maybe<RuntimeTestAccessRole> GetRequestAccessRoles(HttpRequestBase request)
        {
            var roles = RuntimeTestAccessRole.Guest;

            if (request.QueryString["webadmin"] != null)
            {
                roles |= RuntimeTestAccessRole.WebAdmins;
            }
            if (request.QueryString["sysadmin"] != null)
            {
                roles |= RuntimeTestAccessRole.SystemAdmins;
            }

            return new Maybe<RuntimeTestAccessRole>(roles);
        }
        #endregion

        public FileResult GetScript()
        {
            var filepath = Path.GetFullPath($@"{HostingEnvironment.MapPath("~")}..\HealthCheck.Frontend\dist\healthcheckfrontend.js");
            return new FileStreamResult(new FileStream(filepath, FileMode.Open), "content-disposition");
        }
    }
}
