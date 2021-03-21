using System.Web.Http;

namespace HealthCheck.DevTest.Controllers.RequestLogTestControllers.Api
{
#pragma warning disable IDE0060 // Remove unused parameter
    public class TestApi2Controller : ApiController
    {
        [Route("TestGet")]
        [HttpGet]
        public string Get(int id) => $"GET: {id}";

        public string Post([FromBody] string value)
        {
            return value;
        }
    }
#pragma warning restore IDE0060 // Remove unused parameter
}
