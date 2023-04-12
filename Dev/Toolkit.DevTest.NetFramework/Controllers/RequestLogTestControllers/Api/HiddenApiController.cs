using QoDL.Toolkit.Core.Attributes;
using System.Collections.Generic;
using System.Web.Http;

namespace QoDL.Toolkit.DevTest.Controllers.RequestLogTestControllers.Api;

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
        if (id == -1)
        {
            _ = int.Parse("err pls");
        }
        return "value";
    }

    // POST: api/student
    public void Post([FromBody] string value)
    {
        // Method intentionally left empty.
    }

    // PUT: api/student/5
    public void Put(int id, [FromBody] string value)
    {
        // Method intentionally left empty.
    }

    // DELETE: api/student/5
    public void Delete(int id)
    {
        // Method intentionally left empty.
    }
}
