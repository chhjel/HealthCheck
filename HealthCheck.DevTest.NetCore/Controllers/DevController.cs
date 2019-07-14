using HealthCheck.Core.Util;
using HealthCheck.DevTest._TestImplementation;
using HealthCheck.WebUI.Models;
using HealthCheck.WebUI.ViewModels;
using HealthCheck.WebUI.Abstractions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Collections.Generic;

namespace HealthCheck.DevTest.NetCore.Controllers
{
    [Route("/")]
    public class DevController : HealthCheckControllerBase<RuntimeTestAccessRole>
    {
        private readonly IHostingEnvironment _env;
        private const string EndpointBase = "/";

        public DevController(IHostingEnvironment env)
            : base(assemblyContainingTests: typeof(DevController).Assembly)
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

        protected override void Configure(HttpRequest request)
        {
            TestRunner.IncludeExceptionStackTraces = CurrentRequestAccessRoles.HasValue && CurrentRequestAccessRoles.Value.HasFlag(RuntimeTestAccessRole.SystemAdmins);
        }

        protected override void SetTestSetGroupsOptions(TestSetGroupsOptions options)
        {
            options.SetOptionsFor(RuntimeTestConstants.Group.AdminStuff, uiOrder: -100);
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
