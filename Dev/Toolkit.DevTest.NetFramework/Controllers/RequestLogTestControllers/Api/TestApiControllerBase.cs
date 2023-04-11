using System.Collections.Generic;
using System.Web.Http;

namespace QoDL.Toolkit.DevTest.Controllers.RequestLogTestControllers.Api
{
    public abstract class TestApiControllerBase : ApiController
    {
        // GET: api/student
        public IEnumerable<string> SomeBaseAction()
        {
            return new string[] { "value1", "value2" };
        }

        // DELETE: api/student/5
        public void AnotherBaseAction(int id)
        {
            // Method intentionally left empty.
        }
    }
}
