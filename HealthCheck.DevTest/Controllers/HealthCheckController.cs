using System.IO;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;
using HealthCheck.Core.TestManagers;
using HealthCheck.Core.Util;
using HealthCheck.DevTest._TestImplementation;
using HealthCheck.Web.Core.Models;
using HealthCheck.Web.Core.ViewModels;

namespace HealthCheck.DevTest.Controllers
{
    public class HealthCheckController : HealthCheckControllerBase<RuntimeTestAccessRole>
    {
        public HealthCheckController()
            : base(assemblyContainingTests: typeof(HealthCheckController).Assembly) {}


        #region Overrides
        protected override FrontEndOptionsViewModel GetFrontEndOptions()
            => new FrontEndOptionsViewModel()
            {
                ExecuteTestEndpoint = "/HealthCheck/ExecuteTest",
                GetTestsEndpoint = "/HealthCheck/GetTests",
                ApplicationTitle = "Site Status"
            };

        protected override PageOptions GetPageOptions()
            => new PageOptions()
            {
                JavaScriptUrl = "/HealthCheck/GetScript",
                PageTitle = "Dev Checks"
            };

        protected override void SetOptionalOptions(HttpRequestBase request, TestRunner testRunner, TestDiscoverer testDiscoverer)
        {
            var requestRoles = GetRequestAccessRoles(request);
            testRunner.IncludeExceptionStackTraces = requestRoles.HasValue && requestRoles.Value.HasFlag(RuntimeTestAccessRole.SystemAdmins);
            testDiscoverer.GroupOptions
                .SetOptionsFor(RuntimeTestConstants.Group.Test, uiOrder: -100, iconName: RuntimeTestConstants.Icons.Face);
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
