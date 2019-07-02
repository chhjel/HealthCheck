using HealthCheck.Core.Entities;
using HealthCheck.Core.TestManagers;
using HealthCheck.Core.Util;
using HealthCheck.Core.Attributes;
using HealthCheck.Web.Core.Factories;
using HealthCheck.Web.Core.ViewModels;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Hosting;
using System.Web.Mvc;
using Newtonsoft.Json.Converters;

namespace HealthCheck.DevTest.Controllers
{
    public abstract class HealthCheckControllerBase : Controller
    {
        private readonly TestViewModelsFactory _testViewModelsFactory = new TestViewModelsFactory();
        private readonly TestRunner _testRunner = new TestRunner();
        private readonly StringConverter _stringConverter = new StringConverter();


        /// <summary>
        /// Should return a custom enum flag object with the roles of the current user. Must match the type used in <see cref="RuntimeTestAttribute.RolesWithAccess"/>.
        /// <para>Returns null by default to allow all test.</para>
        /// </summary>
        protected virtual object GetRequestAccessRoles() => null;

        private const string Q = "\"";
        public virtual ActionResult Index()
        {
            //if (Config.AccessCheck?.Invoke(Request) == false || CurrentReleaseJson == null) return HttpNotFound();
            var javascriptUrl = "/HealthCheck/GetScript";

            return Content($@"
<!doctype html>
<html>
<head>
    <meta name={Q}robots{Q} content={Q}noindex{Q}>
    <meta name={Q}viewport{Q} content={Q}width=device-width, initial-scale=1, maximum-scale=1, user-scalable=no, minimal-ui{Q}>
    <link href={Q}https://cdn.jsdelivr.net/npm/vuetify@1/dist/vuetify.min.css{Q} rel={Q}stylesheet{Q} />
    <link href='https://fonts.googleapis.com/css?family=Roboto:100,300,400,500,700,900|Material+Icons' rel={Q}stylesheet{Q} />
    <link href={Q}https://use.fontawesome.com/releases/v5.7.2/css/all.css{Q} rel={Q}stylesheet{Q} integrity={Q}sha384-fnmOCqbTlWIlj8LyTjo7mOUStjsKC4pOpQbqyi7RrhN7udi9RwhKkMHpvLbHG9Sr{Q} crossorigin={Q}anonymous{Q}>
</head>

<body>
    <div id={Q}app{Q}></div>

    <script src={Q}{javascriptUrl}{Q}></script>
</body>
</html>");
        }

        public FileResult GetScript()
        {
            var filepath = Path.GetFullPath($@"{HostingEnvironment.MapPath("~")}..\HealthCheck.Frontend\dist\healthcheckfrontend.js");
            return new FileStreamResult(new FileStream(filepath, FileMode.Open), "content-disposition");
        }

        public ActionResult GetTests()
        {
            var viewModel = _testViewModelsFactory.CreateViewModel(GetTestDefinitions());
            return CreateJsonResult(viewModel);
        }

        [HttpPost]
        public async Task<ActionResult> ExecuteTest(ExecuteTestInputData data)
        {
            if (data == null || data.TestId == null)
            {
                return CreateJsonResult(TestResultViewModel.CreateError("No test id was given."));
            }

            var test = GetTest(data.TestId);
            if (test == null)
            {
                return CreateJsonResult(TestResultViewModel.CreateError($"Test with id '{data.TestId}' not found.", data.TestId));
            }

            try
            {
                var parameters = data?.GetParametersWithConvertedTypes(test.Parameters.Select(x => x.ParameterType).ToArray(), _stringConverter);
                var result = await _testRunner.ExecuteTest(test, parameters);
                var resultVm = _testViewModelsFactory.CreateViewModel(result);
                return CreateJsonResult(resultVm);
            }
            catch (Exception ex)
            {
                var message = $"Exception: {(ex.InnerException ?? ex).Message}";
                return CreateJsonResult(TestResultViewModel.CreateError(message));
            }
        }

        public class ExecuteTestInputData
        {
            public string TestId { get; set; }
            public List<string> Parameters { get; set; }

            public object[] GetParametersWithConvertedTypes(Type[] types, StringConverter stringConverter)
            {
                var objects = new object[types.Length];
                for(int i = 0; i < objects.Length; i++)
                {
                    var type = types[i];
                    var convertedObject = stringConverter.ConvertStringTo(type, Parameters[i]);
                    objects[i] = convertedObject;
                }
                return objects;
            }
        }

        private ActionResult CreateJsonResult(object obj)
        {
            var settings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented
            };
            settings.Converters.Add(new StringEnumConverter());

            var json = JsonConvert.SerializeObject(obj, settings);
            return Content(json, "application/json");
        }

        private List<TestClassDefinition> _testCache;
        private List<TestClassDefinition> GetTestDefinitions()
        {
            if (_testCache != null)
            {
                return _testCache;
            }

            var userRoles = GetRequestAccessRoles();
            _testCache = new TestDiscoverer()
            {
                AssemblyContainingTests = GetType().Assembly
            }
            .DiscoverTestDefinitions(onlyTestsAllowedToBeManuallyExecuted: true, userRolesEnum: userRoles);

            return _testCache;
        }

        private TestDefinition GetTest(string testId)
            => GetTestDefinitions().SelectMany(x => x.Tests).FirstOrDefault(x => x.Id == testId);
    }
}