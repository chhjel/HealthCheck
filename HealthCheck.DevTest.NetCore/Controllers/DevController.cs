using HealthCheck.DevTest._TestImplementation;
using HealthCheck.WebUI.Abstractions;
using HealthCheck.WebUI.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.IO;

namespace HealthCheck.DevTest.NetCore.Controllers
{
    [Route("/")]
    public class DevController : HealthCheckControllerBase<RuntimeTestAccessRole>
    {
        private readonly IHostingEnvironment _env;
        private const string EndpointBase = "/";

        public DevController(IHostingEnvironment env) : base()
        {
            _env = env;
        }

        #region Overrides
        protected override FrontEndOptionsViewModel GetFrontEndOptions()
            => new FrontEndOptionsViewModel(EndpointBase)
            {
                ApplicationTitle = "Site Status"
            };

        protected override PageOptions GetPageOptions()
            => new PageOptions()
            {
                JavaScriptUrls = new List<string> {
                    $"{EndpointBase.TrimEnd('/')}/GetVendorScript",
                    $"{EndpointBase.TrimEnd('/')}/GetMainScript",
                },
                PageTitle = "Dev Checks"
            };

        protected override void ConfigureAccess(HttpRequest request, AccessConfig<RuntimeTestAccessRole> options)
        {
            options.RedirectTargetOnNoAccess = "/no-access";
        }

        protected override RequestInformation<RuntimeTestAccessRole> GetRequestInformation(HttpRequest request)
        {
            var roles = RuntimeTestAccessRole.Guest;

            if (request.Query.ContainsKey("webadmin"))
            {
                roles |= RuntimeTestAccessRole.WebAdmins;
            }
            if (request.Query.ContainsKey("sysadmin"))
            {
                roles |= RuntimeTestAccessRole.SystemAdmins;
            }

            return new RequestInformation<RuntimeTestAccessRole>(roles, "dev42core", "Dev core user");
        }
        #endregion

        [Route("GetMainScript")]
        public FileResult GetMainScript()
        {
            var filepath = Path.GetFullPath(Path.Combine(_env.WebRootPath, @"..\..\HealthCheck.Frontend\dist\healthcheck.js"));
            return new FileStreamResult(new FileStream(filepath, FileMode.Open), "text/javascript");
        }

        [Route("GetVendorScript")]
        public FileResult GetVendorScript()
        {
            var filepath = Path.GetFullPath(Path.Combine(_env.WebRootPath, @"..\..\HealthCheck.Frontend\dist\healthcheck.vendor.js"));
            return new FileStreamResult(new FileStream(filepath, FileMode.Open), "text/javascript");
        }
    }
}
