using HealthCheck.Core.Attributes;
using System.Web.Http;

namespace HealthCheck.DevTest.Controllers.RequestLogTestControllers.Api
{
#pragma warning disable IDE0060 // Remove unused parameter
    public class TestApiController : TestApiControllerBase
    {
        [Route("TestGet")]
        [HttpGet]
        public string TestGet(int id) => $"GET: {id}";

        [Route("TestPost")]
        [HttpPost]
        public string TestPost() => "POST";

        // GET: api/student/5
        public string Get(int id)
        {
            if (id == -1) int.Parse("err pls");
            return "value";
        }

        // POST: api/student
        public void Post([FromBody]string value)
        {
        }

        // PUT: api/student/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE: api/student/5
        public void Delete(int id)
        {
        }


        [HideFromRequestLog]
        public void HiddenEndpoint(int id)
        {
        }
    }
#pragma warning restore IDE0060 // Remove unused parameter
}
