using HealthCheck.Core.Attributes;
using System.Collections.Generic;
using System.Web.Http;

#pragma warning disable IDE0060 // Remove unused parameter
namespace HealthCheck.DevTest.Controllers.RequestLogTestControllers.Api
{
    [HideFromRequestLog]
    public class HiddenApiController : TestApiControllerBase
    {
        // GET: api/student
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

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
    }
}
#pragma warning restore IDE0060 // Remove unused parameter
