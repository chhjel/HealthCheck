using QoDL.Toolkit.Core.Attributes;
using System.Web.Http;

namespace QoDL.Toolkit.DevTest.Controllers.RequestLogTestControllers.Api;

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


    [HideFromRequestLog]
    public void HiddenEndpoint(int id)
    {
        // Method intentionally left empty.
    }
}
